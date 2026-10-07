using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el acceso a las fotos de perfil.
/// Su responsabilidad es leer, guardar y quitar la foto de una cuenta. Todas sus operaciones
/// reciben el identificador de la cuenta con sesión.
/// No ofrece listar fotos ni buscar la de otra persona por ningún otro dato (RF-038).
/// </summary>
public interface IRepositorioFotosPerfil
{
    /// <summary>La foto de la cuenta, o nulo si no tiene.</summary>
    Task<FotoPerfil?> ObtenerAsync(Guid usuarioId, CancellationToken cancelacion = default);

    /// <summary>Guarda la foto de la cuenta, reemplazando la que hubiera; se guarda con la unidad de trabajo.</summary>
    Task GuardarAsync(Guid usuarioId, byte[] contenido, string tipoContenido, CancellationToken cancelacion = default);

    /// <summary>Quita la foto de la cuenta, si la tiene; se guarda con la unidad de trabajo.</summary>
    Task QuitarAsync(Guid usuarioId, CancellationToken cancelacion = default);
}
