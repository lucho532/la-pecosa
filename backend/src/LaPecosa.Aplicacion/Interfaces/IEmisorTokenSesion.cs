using LaPecosa.Aplicacion.Utilidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa la emisión de los tokens de sesión (research §4).
/// Su responsabilidad es firmar un token con el identificador del usuario y su sello de seguridad.
/// No incluye en el token ni el club ni el rol, y no valida tokens recibidos.
/// </summary>
public interface IEmisorTokenSesion
{
    /// <summary>Emite un token para el usuario con su sello de seguridad actual.</summary>
    TokenEmitido Emitir(Guid usuarioId, Guid selloSeguridad);
}
