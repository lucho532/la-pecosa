using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa una invitación tal como la ve el club que la envió (RF-005).
/// Su responsabilidad es mostrar a quién se envió, en qué estado está, si el correo salió, quién
/// la envió y cuándo vence.
/// Nunca incluye el token ni su hash, ni el rol: la invitación del club no lleva rol.
/// </summary>
/// <param name="InvitacionId">Identificador de la invitación.</param>
/// <param name="Correo">Correo invitado.</param>
/// <param name="Estado">Pendiente, usada, vencida o cancelada.</param>
/// <param name="EstadoEnvio">Resultado del envío del correo.</param>
/// <param name="EnviadaPor">Nombre de quien la envió; nulo si ya no está en el club.</param>
/// <param name="CreadaEn">Fecha de envío, en UTC.</param>
/// <param name="VenceEn">Fecha de vencimiento, en UTC.</param>
public record InvitacionClubDto(
    Guid InvitacionId,
    string Correo,
    EstadoInvitacion Estado,
    EstadoEnvio EstadoEnvio,
    string? EnviadaPor,
    DateTime CreadaEn,
    DateTime VenceEn);
