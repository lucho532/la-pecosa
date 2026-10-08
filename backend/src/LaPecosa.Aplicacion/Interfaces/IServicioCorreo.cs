using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el envío de los correos de la plataforma (constitución §6.3).
/// Su responsabilidad es enviar la invitación a un club y el enlace de recuperación de contraseña.
/// No crea invitaciones ni solicitudes, y nunca guarda el token que recibe.
/// </summary>
public interface IServicioCorreo
{
    /// <summary>
    /// Envía la invitación; devuelve si se pudo enviar. Con el rol PRESIDENTE el correo nombra ese
    /// rol; con cualquier otro es una invitación del club y el correo no nombra ningún rol, porque
    /// se decide al aprobar el ingreso (RF-003).
    /// </summary>
    Task<bool> EnviarInvitacionAsync(
        string correo, string nombreClub, Rol rol, string token, CancellationToken cancelacion = default);

    /// <summary>Envía el enlace de recuperación de contraseña; devuelve si se pudo enviar.</summary>
    Task<bool> EnviarRecuperacionAsync(string correo, string token, CancellationToken cancelacion = default);
}
