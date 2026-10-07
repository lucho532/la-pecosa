using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios;

/// <summary>
/// Representa el acceso a las cuentas con Entity Framework.
/// Su responsabilidad es buscar, añadir y eliminar cuentas.
/// No contiene reglas de negocio ni consulta datos de ningún club.
/// </summary>
public class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioUsuarios(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public Task<Usuario?> ObtenerPorIdAsync(Guid usuarioId, CancellationToken cancelacion = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == usuarioId, cancelacion);

    /// <inheritdoc />
    public Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken cancelacion = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.CorreoNormalizado == correoNormalizado, cancelacion);

    /// <inheritdoc />
    public Task<Usuario?> ObtenerDesarrolladorAsync(CancellationToken cancelacion = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.EsDesarrollador, cancelacion);

    /// <inheritdoc />
    public Task RegistrarFalloDeSesionAsync(Guid usuarioId, CancellationToken cancelacion = default) =>
        _contexto.Usuarios
            .Where(usuario => usuario.Id == usuarioId && !usuario.Bloqueada)
            .ExecuteUpdateAsync(
                cambios => cambios
                    .SetProperty(usuario => usuario.IntentosFallidos, usuario => usuario.IntentosFallidos + 1)
                    .SetProperty(
                        usuario => usuario.Bloqueada,
                        usuario => usuario.IntentosFallidos + 1 >= Usuario.IntentosParaBloquear),
                cancelacion);

    /// <inheritdoc />
    public Task ReiniciarFallosDeSesionAsync(Guid usuarioId, CancellationToken cancelacion = default) =>
        _contexto.Usuarios
            .Where(usuario => usuario.Id == usuarioId && !usuario.Bloqueada && usuario.IntentosFallidos > 0)
            .ExecuteUpdateAsync(
                cambios => cambios.SetProperty(usuario => usuario.IntentosFallidos, 0), cancelacion);

    /// <inheritdoc />
    public void Agregar(Usuario usuario) => _contexto.Usuarios.Add(usuario);

    /// <inheritdoc />
    public void Eliminar(Usuario usuario) => _contexto.Usuarios.Remove(usuario);
}
