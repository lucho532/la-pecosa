using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios.Plataforma;

/// <summary>
/// Representa el acceso a los clubes desde el panel del DESARROLLADOR (constitución §7.1, primera
/// excepción).
/// Su responsabilidad es leer y guardar clubes y sus presidentes. Se salta el filtro de
/// aislamiento porque el panel trabaja sobre todos los clubes, y por eso solo consulta
/// integrantes con rol PRESIDENTE.
/// No expone ningún otro dato de un club (§8) ni contiene reglas de negocio.
/// </summary>
public class RepositorioClubesPlataforma : IRepositorioClubesPlataforma
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioClubesPlataforma(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    private IQueryable<UsuarioRol> Presidentes => _contexto.UsuariosRol
        .IgnoreQueryFilters()
        .Where(integrante => integrante.Rol == Rol.PRESIDENTE);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClubConPresidente>> ListarAsync(CancellationToken cancelacion = default) =>
        await _contexto.Clubes
            .OrderBy(club => club.Nombre)
            .Select(club => new ClubConPresidente(
                club,
                Presidentes.Any(integrante => integrante.ClubId == club.Id)))
            .ToListAsync(cancelacion);

    /// <inheritdoc />
    public Task<Club?> ObtenerAsync(Guid clubId, CancellationToken cancelacion = default) =>
        _contexto.Clubes.FirstOrDefaultAsync(club => club.Id == clubId, cancelacion);

    /// <inheritdoc />
    public Task<bool> ExisteNombreAsync(
        string nombreNormalizado, Guid? exceptoClubId = null, CancellationToken cancelacion = default) =>
        _contexto.Clubes.AnyAsync(
            club => club.NombreNormalizado == nombreNormalizado && club.Id != exceptoClubId, cancelacion);

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsuarioRol>> ListarPresidentesAsync(
        Guid clubId, CancellationToken cancelacion = default) =>
        await Presidentes
            .Include(integrante => integrante.Usuario)
            .Where(integrante => integrante.ClubId == clubId)
            .OrderBy(integrante => integrante.CreadoEn)
            .ToListAsync(cancelacion);

    /// <inheritdoc />
    public Task<bool> EsPresidenteAsync(
        Guid clubId, string correoNormalizado, CancellationToken cancelacion = default) =>
        Presidentes.AnyAsync(
            integrante => integrante.ClubId == clubId && integrante.Usuario!.CorreoNormalizado == correoNormalizado,
            cancelacion);

    /// <inheritdoc />
    public void Agregar(Club club) => _contexto.Clubes.Add(club);
}
