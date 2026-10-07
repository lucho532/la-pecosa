using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de invitar a un presidente (constitución §12.5, RF-012).
/// Su responsabilidad es crear la invitación guardando solo el hash de su token, enviarla por
/// correo después de confirmar la transacción y registrar si el envío salió o falló; además de
/// invitar a un presidente adicional y reenviar una invitación sin usar.
/// No crea cuentas ni asigna contraseñas: solo envía invitaciones (§8). Ningún método devuelve el
/// token a la API.
/// </summary>
public interface IServicioInvitacionPresidente
{
    /// <summary>
    /// Comprueba que a ese correo se le puede invitar: 409 <c>correo_del_desarrollador</c> si es el
    /// de la cuenta DESARROLLADOR, que no pertenece a ningún club.
    /// </summary>
    Task ComprobarCorreoInvitableAsync(string correoNormalizado, CancellationToken cancelacion = default);

    /// <summary>
    /// Prepara, dentro de la transacción en curso, una invitación de presidente para el club: anula
    /// las pendientes de ese correo y añade la nueva. Devuelve la invitación y su token en claro,
    /// que solo sirve para enviarlo por correo.
    /// </summary>
    Task<(Invitacion Invitacion, string Token)> PrepararAsync(
        Club club, string correoNormalizado, Guid creadaPorUsuarioId, CancellationToken cancelacion = default);

    /// <summary>Envía el correo, ya confirmada la transacción, y guarda si salió o falló.</summary>
    Task EnviarAsync(Invitacion invitacion, string nombreClub, string token, CancellationToken cancelacion = default);

    /// <summary>Invita a un presidente a un club que ya existe.</summary>
    Task<InvitacionDto> InvitarAsync(
        Guid clubId, InvitarPresidenteDto datos, Guid usuarioId, CancellationToken cancelacion = default);

    /// <summary>Anula una invitación sin usar y crea otra, al mismo correo o al corregido.</summary>
    Task<InvitacionDto> ReenviarAsync(
        Guid clubId, Guid invitacionId, ReenviarInvitacionDto? datos, Guid usuarioId, CancellationToken cancelacion = default);
}
