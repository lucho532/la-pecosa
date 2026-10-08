using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa un ingreso aprobado de la lista de solo lectura del club (RF-025 y RF-025a).
/// Su responsabilidad es decir quién entró, con qué rol, quién lo aprobó y cuándo, tal como quedó
/// registrado en ese momento.
/// No refleja el rol actual de la persona ni el nombre actual de quien aprobó (§13), y no permite
/// modificar nada.
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
