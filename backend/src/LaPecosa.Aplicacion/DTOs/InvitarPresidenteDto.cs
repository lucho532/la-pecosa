namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa la invitación de un presidente a un club que ya existe.
/// Su responsabilidad es llevar el correo al que se envía.
/// No lleva el rol: en esta funcionalidad siempre es PRESIDENTE.
/// </summary>
/// <param name="Correo">Correo al que se envía la invitación.</param>
public record InvitarPresidenteDto(
    string? Correo);
