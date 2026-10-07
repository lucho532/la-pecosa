using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios;

/// <summary>
/// Representa el acceso a las solicitudes de recuperación de contraseña con Entity Framework.
/// Su responsabilidad es guardarlas, encontrarlas por hash y dejar sin efecto las anteriores.
/// No decide si una solicitud está vigente.
/// </summary>
public class RepositorioSolicitudesRecuperacion : IRepositorioSolicitudesRecuperacion
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioSolicitudesRecuperacion(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public Task<SolicitudRecuperacion?> ObtenerPorHashAsync(string tokenHash, CancellationToken cancelacion = default) =>
        _contexto.SolicitudesRecuperacion
            .Include(solicitud => solicitud.Usuario)
            .FirstOrDefaultAsync(solicitud => solicitud.TokenHash == tokenHash, cancelacion);

    /// <inheritdoc />
    public Task AnularPendientesAsync(Guid usuarioId, DateTime ahoraUtc, CancellationToken cancelacion = default) =>
        _contexto.SolicitudesRecuperacion
            .Where(solicitud => solicitud.UsuarioId == usuarioId && solicitud.UsadaEn == null)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(solicitud => solicitud.UsadaEn, ahoraUtc), cancelacion);

    /// <inheritdoc />
    public void Agregar(SolicitudRecuperacion solicitud) => _contexto.SolicitudesRecuperacion.Add(solicitud);
}
