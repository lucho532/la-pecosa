using LaPecosa.Api.Errores;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Dominio.Reglas;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LaPecosa.Api.Autorizacion;

/// <summary>
/// Representa la autorización de los endpoints <c>/api/clubes/{clubId}/**</c> (research §3).
/// Su responsabilidad es comprobar en cada petición, contra la base de datos, que la cuenta tiene
/// un integrante en el club de la ruta, que el estado del club le permite entrar y que su ingreso
/// ya fue aprobado; si pasa, fija el club en <see cref="IContextoClub"/> y deja el integrante
/// disponible para el controlador. Puede exigir además uno de varios roles.
/// No confía en el token para el club ni el rol, y no revela que un club existe a quien no
/// pertenece a él: responde el mismo 404 que para un club inexistente, también al DESARROLLADOR.
/// No abre ningún endpoint a quien está en la sala de espera: se le niega por defecto, antes de
/// mirar los roles, así que cualquier endpoint de club futuro queda cubierto (research §4).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IntegranteDelClubAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string ClaveIntegrante = "LaPecosa.IntegranteDeLaPeticion";

    private readonly Rol[] _rolesExigidos;

    /// <summary>
    /// Exige ser integrante aprobado del club con uno de los roles indicados; sin roles, con
    /// cualquiera.
    /// </summary>
    public IntegranteDelClubAttribute(params Rol[] rolesExigidos)
    {
        _rolesExigidos = rolesExigidos;
    }

    /// <summary>Integrante de la cuenta en el club de la petición, ya comprobado.</summary>
    public static UsuarioRol IntegranteDe(HttpContext contexto) =>
        contexto.Items[ClaveIntegrante] as UsuarioRol
        ?? throw new InvalidOperationException("El endpoint no está protegido con [IntegranteDelClub].");

    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var usuario = ValidadorSesion.UsuarioDe(http);
        if (usuario is null)
        {
            context.Result = Problema.SinSesion().ComoResultado();
            return;
        }

        if (!Guid.TryParse(http.Request.RouteValues["clubId"]?.ToString(), out var clubId))
        {
            context.Result = Problema.NoEncontrado().ComoResultado();
            return;
        }

        var integrante = await http.RequestServices.GetRequiredService<IRepositorioPertenencias>()
            .ObtenerAsync(usuario.Id, clubId, http.RequestAborted);
        if (integrante?.Club is null)
        {
            context.Result = Problema.NoEncontrado().ComoResultado();
            return;
        }

        var acceso = ReglaAccesoPorEstado.Evaluar(integrante.Club.Estado, integrante.Rol, integrante.EstadoIngreso);
        var problema = acceso switch
        {
            ResultadoAcceso.ClubSuspendido => new Problema(
                403, "club_suspendido", "Hay una incidencia temporal. Comunícate con el presidente del club."),
            ResultadoAcceso.ClubDadoDeBaja => new Problema(403, "club_dado_de_baja", "Este club no está disponible."),
            ResultadoAcceso.IngresoEnEspera => new Problema(
                403, "ingreso_en_espera", "Tu ingreso está pendiente de aprobación."),
            _ when _rolesExigidos.Length > 0 && !_rolesExigidos.Contains(integrante.Rol) => new Problema(
                403, "rol_no_autorizado", "Tu rol en este club no permite hacer esto."),
            _ => null,
        };

        if (problema is not null)
        {
            context.Result = problema.ComoResultado();
            return;
        }

        http.RequestServices.GetRequiredService<IContextoClub>().Fijar(clubId);
        http.Items[ClaveIntegrante] = integrante;
    }
}
