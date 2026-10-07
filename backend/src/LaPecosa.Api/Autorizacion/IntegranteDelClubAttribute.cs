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
/// un integrante en el club de la ruta y que el estado del club le permite entrar; si pasa, fija
/// el club en <see cref="IContextoClub"/> y deja el integrante disponible para el controlador.
/// Puede exigir además un rol concreto.
/// No confía en el token para el club ni el rol, y no revela que un club existe a quien no
/// pertenece a él: responde el mismo 404 que para un club inexistente, también al DESARROLLADOR.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IntegranteDelClubAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string ClaveIntegrante = "LaPecosa.IntegranteDeLaPeticion";

    private readonly Rol? _rolExigido;

    /// <summary>Exige ser integrante del club, con cualquier rol.</summary>
    public IntegranteDelClubAttribute()
    {
    }

    /// <summary>Exige ser integrante del club con el rol indicado.</summary>
    public IntegranteDelClubAttribute(Rol rolExigido)
    {
        _rolExigido = rolExigido;
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

        var problema = ReglaAccesoPorEstado.Evaluar(integrante.Club.Estado, integrante.Rol) switch
        {
            ResultadoAcceso.ClubSuspendido => new Problema(
                403, "club_suspendido", "Hay una incidencia temporal. Comunícate con el presidente del club."),
            ResultadoAcceso.ClubDadoDeBaja => new Problema(403, "club_dado_de_baja", "Este club no está disponible."),
            _ when _rolExigido is not null && integrante.Rol != _rolExigido => new Problema(
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
