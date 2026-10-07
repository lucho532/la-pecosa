using LaPecosa.Aplicacion.DTOs;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de la sesión (constitución §12.4).
/// Su responsabilidad es iniciar sesión con correo o documento, decir quién es la cuenta con
/// sesión y a qué clubes pertenece, y renovar el token.
/// No registra cuentas, no cambia contraseñas y no decide el acceso a un club concreto.
/// </summary>
public interface IServicioSesion
{
    /// <summary>
    /// Inicia sesión. Cuenta inexistente, contraseña incorrecta y cuenta bloqueada fallan con el
    /// mismo 401 <c>credenciales_invalidas</c>.
    /// </summary>
    Task<TokenSesionDto> IniciarAsync(IniciarSesionDto datos, CancellationToken cancelacion = default);

    /// <summary>Devuelve la cuenta y sus clubes; lista vacía para el DESARROLLADOR.</summary>
    Task<SesionDto> ObtenerAsync(Guid usuarioId, CancellationToken cancelacion = default);

    /// <summary>Cambia un token válido por uno nuevo.</summary>
    Task<TokenSesionDto> RenovarAsync(Guid usuarioId, CancellationToken cancelacion = default);
}
