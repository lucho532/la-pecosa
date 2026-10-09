using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa un ingreso aprobado de la lista de solo lectura del club, que solo ve su PRESIDENTE
/// (RF-019).
/// Su responsabilidad es decir quién entró, con qué rol, quién lo aprobó y cuándo, tal como quedó
/// registrado en ese momento. En las aprobaciones nuevas el rol es siempre JUGADOR; las anteriores
/// conservan el suyo y a quien las aprobó, aunque fuera un DIRECTIVO.
/// No refleja el rol actual de la persona ni el nombre actual de quien aprobó (§13), no incluye a
/// quien entró con una invitación y no permite modificar nada.
/// </summary>
/// <param name="UsuarioRolId">Identificador del integrante en este club.</param>
/// <param name="Nombres">Nombres de la persona aprobada.</param>
/// <param name="Apellidos">Apellidos de la persona aprobada.</param>
/// <param name="RolDeIngreso">Rol con el que quedó al aprobarse.</param>
/// <param name="AprobadoPor">Nombre de quien aprobó, tal como era en ese momento.</param>
/// <param name="AprobadoEn">Fecha y hora de la aprobación, en UTC.</param>
public record IngresoAprobadoDto(
    Guid UsuarioRolId,
    string Nombres,
    string Apellidos,
    Rol RolDeIngreso,
    string AprobadoPor,
    DateTime AprobadoEn);
