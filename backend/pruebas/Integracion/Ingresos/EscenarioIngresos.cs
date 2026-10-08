using System.Text.Json;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Pruebas.Integracion.Base;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Pruebas.Integracion.Ingresos;

/// <summary>
/// Ayudas comunes a las pruebas de aprobación, rechazo y consulta de ingresos: rutas, clientes con
/// la sesión de un integrante nuevo y lectura directa de un integrante en la base de datos.
/// </summary>
internal static class EscenarioIngresos
{
    public static string EnEspera(Club club) => $"/api/clubes/{club.Id}/ingresos/en-espera";

    public static string Aprobados(Club club) => $"/api/clubes/{club.Id}/ingresos/aprobados";

    public static string Aprobacion(Club club, Guid usuarioRolId) =>
        $"/api/clubes/{club.Id}/ingresos/{usuarioRolId}/aprobacion";

    public static string Rechazo(Club club, Guid usuarioRolId) =>
        $"/api/clubes/{club.Id}/ingresos/{usuarioRolId}/rechazo";

    /// <summary>Crea un integrante aprobado con ese rol y devuelve un cliente con su sesión.</summary>
    public static async Task<ClienteDePrueba> ClienteDeAsync(this FabricaApi fabrica, Club club, Rol rol)
    {
        var (usuario, _) = await fabrica.Sembrador.CrearIntegranteAsync(club, rol);
        return await fabrica.CrearClienteDePrueba().ConSesionDeAsync(usuario);
    }

    /// <summary>El integrante tal como está guardado, o nulo si ya no existe.</summary>
    public static Task<UsuarioRol?> IntegranteAsync(this FabricaApi fabrica, Guid usuarioRolId) =>
        fabrica.ConContextoAsync(contexto => contexto.UsuariosRol
            .IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(integrante => integrante.Id == usuarioRolId));

    /// <summary>Indica si la cuenta sigue existiendo.</summary>
    public static Task<bool> ExisteCuentaAsync(this FabricaApi fabrica, Guid usuarioId) =>
        fabrica.ConContextoAsync(contexto => contexto.Usuarios.AnyAsync(usuario => usuario.Id == usuarioId));

    /// <summary>Lee una lista JSON de la API.</summary>
    public static async Task<List<JsonElement>> ListaAsync(this ClienteDePrueba cliente, string ruta) =>
        (await ClienteDePrueba.LeerAsync<JsonElement>(await cliente.GetAsync(ruta))).EnumerateArray().ToList();
}
