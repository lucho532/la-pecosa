using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios.Plataforma;

/// <summary>
/// Representa el acceso a las invitaciones desde el panel del DESARROLLADOR (constitución §7.1,
/// primera excepción).
/// Su responsabilidad es guardar, listar y anular las invitaciones de un club. Se salta el filtro
/// de aislamiento porque el panel no tiene un club en el contexto; cada consulta va acotada al
/// club indicado.
/// No busca invitaciones por token ni contiene reglas de negocio.
/// </summary>
public class RepositorioInvitacionesPlataforma : IRepositorioInvitacionesPlataforma
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioInvitacionesPlataforma(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    private IQueryable<Invitacion> Pendientes(Guid clubId) => _contexto.Invitaciones
        .IgnoreQueryFilters()
        .Where(invitacion => invitacion.ClubId == clubId && invitacion.UsadaEn == null && invitacion.AnuladaEn == null);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Invitacion>> ListarPendientesAsync(
        Guid clubId, CancellationToken cancelacion = default) =>
        await Pendientes(clubId).OrderByDescending(invitacion => invitacion.CreadaEn).ToListAsync(cancelacion);

    /// <inheritdoc />
    public Task<Invitacion?> ObtenerAsync(Guid clubId, Guid invitacionId, CancellationToken cancelacion = default) =>
        _contexto.Invitaciones
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                invitacion => invitacion.ClubId == clubId && invitacion.Id == invitacionId, cancelacion);

    /// <inheritdoc />
    public Task AnularPendientesAsync(
        Guid clubId, string correoNormalizado, DateTime ahoraUtc, CancellationToken cancelacion = default) =>
        Pendientes(clubId)
            .Where(invitacion => invitacion.Correo == correoNormalizado)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(invitacion => invitacion.AnuladaEn, ahoraUtc), cancelacion);

    /// <inheritdoc />
    public void Agregar(Invitacion invitacion) => _contexto.Invitaciones.Add(invitacion);
}
