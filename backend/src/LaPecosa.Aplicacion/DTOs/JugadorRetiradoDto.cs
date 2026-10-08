namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa a un jugador retirado en la lista del club (RF-044).
/// Su responsabilidad es decir quién es, su año de nacimiento, quién lo retiró y cuándo, tal como
/// quedó registrado en ese momento.
/// No refleja el nombre actual de quien lo retiró (§13) ni lleva ningún dato de contacto.
/// </summary>
/// <param name="UsuarioRolId">Identificador del integrante en este club.</param>
/// <param name="Nombres">Nombres del jugador.</param>
/// <param name="Apellidos">Apellidos del jugador.</param>
/// <param name="AnioNacimiento">Año de nacimiento.</param>
/// <param name="RetiradoPor">Nombre de quien lo retiró, tal como era en ese momento.</param>
/// <param name="RetiradoEn">Fecha y hora del retiro, en UTC.</param>
public record JugadorRetiradoDto(
    Guid UsuarioRolId,
    string Nombres,
    string Apellidos,
    int AnioNacimiento,
    string RetiradoPor,
    DateTime RetiradoEn);
