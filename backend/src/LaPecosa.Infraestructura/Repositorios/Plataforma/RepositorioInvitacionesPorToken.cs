using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios.Plataforma;

/// <summary>
/// Representa el acceso a una invitación por su enlace (constitución §7.1, tercera excepción).
/// Su responsabilidad es encontrar una invitación por el hash de su token, con su club, y marcarla
/// como usada. Se salta el filtro de aislamiento porque quien abre el enlace aún no pertenece a
/// ningún club; cada consulta va acotada a un único hash o a un único identificador.
/// No lista invitaciones ni busca por club o por correo.
/// </summary>
public class RepositorioInvitacionesPorToken : IRepositorioInvitacionesPorToken
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioInvitacionesPorToken(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public Task<Invitacion?> ObtenerPorHashAsync(string tokenHash, CancellationToken cancelacion = default) =>
        _contexto.Invitaciones
            .IgnoreQueryFilters()
            .Include(invitacion => invitacion.Club)
            .FirstOrDefaultAsync(invitacion => invitacion.TokenHash == tokenHash, cancelacion);

    /// <inheritdoc />
    public async Task<bool> MarcarUsadaAsync(
        Guid invitacionId, DateTime ahoraUtc, CancellationToken cancelacion = default) =>
        await _contexto.Invitaciones
            .IgnoreQueryFilters()
            .Where(invitacion => invitacion.Id == invitacionId
                && invitacion.UsadaEn == null
                && invitacion.AnuladaEn == null
                && invitacion.VenceEn > ahoraUtc)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(invitacion => invitacion.UsadaEn, ahoraUtc), cancelacion) == 1;
}
