namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa los datos para invitar a una persona a registrarse en el club (RF-002).
/// Su responsabilidad es llevar el correo de la persona invitada, y nada más.
/// No lleva rol ni club: el rol se decide al aprobar el ingreso y el club es el de la ruta.
/// </summary>
/// <param name="Correo">Correo al que se envía la invitación.</param>
public record InvitarAlClubDto(string? Correo);
