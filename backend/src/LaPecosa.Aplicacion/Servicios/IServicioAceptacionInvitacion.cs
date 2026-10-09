using LaPecosa.Aplicacion.DTOs;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de aceptar una invitación con una cuenta que ya existe (RF-010).
/// Su responsabilidad es añadir a la cuenta al club de la invitación, con el rol de la invitación
/// y ya aprobada, sin sala de espera y ubicada en su categoría si entra como JUGADOR; o, con una
/// invitación de presidente, reemplazar su rol si ya era integrante de ese club. Nunca crea una
/// segunda cuenta ni un segundo integrante.
/// No sirve para registrarse: exige sesión, y la sesión debe ser la de la cuenta cuyo correo
/// recibió la invitación. No pide ningún dato ni cambia nada en los otros clubes de la persona.
/// </summary>
public interface IServicioAceptacionInvitacion
{
    /// <summary>
    /// Acepta la invitación. Devuelve el club y si se creó una pertenencia nueva (falso cuando la
    /// cuenta ya era integrante y solo cambió su rol). Sesión de otro correo: 403
    /// <c>invitacion_de_otro_correo</c>. Invitación no vigente: 410 <c>invitacion_no_valida</c>.
    /// </summary>
    Task<(ClubDeSesionDto Club, bool Creada)> AceptarAsync(
        TokenDto datos, Guid usuarioId, CancellationToken cancelacion = default);
}
