using LaPecosa.Aplicacion.DTOs;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de registrarse con una invitación (constitución §12.1 y §12.5).
/// Su responsabilidad es mostrar los datos fijos de una invitación vigente y crear, con ella, la
/// cuenta y su integrante en el club y con el rol de la invitación, ya aprobado.
/// No existe ningún otro camino para crear una cuenta, y nadie elige su club, su rol ni su correo:
/// los determina la invitación.
/// </summary>
public interface IServicioRegistroConInvitacion
{
    /// <summary>
    /// Devuelve los datos de la invitación. Usada, vencida, anulada o inexistente: 410
    /// <c>invitacion_no_valida</c>.
    /// </summary>
    Task<InvitacionVigenteDto> ConsultarAsync(TokenDto datos, CancellationToken cancelacion = default);

    /// <summary>Registra la cuenta y su integrante, marca la invitación como usada e inicia la sesión.</summary>
    Task<TokenSesionDto> RegistrarAsync(RegistrarConInvitacionDto datos, CancellationToken cancelacion = default);
}
