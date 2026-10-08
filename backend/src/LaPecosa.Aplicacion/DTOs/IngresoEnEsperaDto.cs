using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa a una persona de la sala de espera tal como la ven el PRESIDENTE y los DIRECTIVOS de
/// su club (RF-020).
/// Su responsabilidad es llevar los datos con los que se registró y cuándo lo hizo, para que el
/// club decida si aprueba o rechaza su ingreso.
/// No sale por ningún otro endpoint: es el único lugar donde la API entrega el nombre del
/// responsable. No lleva rol: en espera siempre es JUGADOR, y el definitivo se elige al aprobar.
/// </summary>
/// <param name="UsuarioRolId">Identificador del integrante en este club.</param>
/// <param name="Nombres">Nombres.</param>
/// <param name="Apellidos">Apellidos.</param>
/// <param name="TipoDocumento">Tipo de documento.</param>
/// <param name="NumeroDocumento">Número de documento.</param>
/// <param name="FechaNacimiento">Fecha de nacimiento.</param>
/// <param name="Correo">Correo de la cuenta.</param>
/// <param name="Celular">Celular de contacto.</param>
/// <param name="NombreResponsable">Nombre del padre, madre o responsable; nulo si no lo indicó.</param>
/// <param name="RegistradoEn">Fecha en que se registró en el club, en UTC.</param>
public record IngresoEnEsperaDto(
    Guid UsuarioRolId,
    string Nombres,
    string Apellidos,
    TipoDocumento TipoDocumento,
    string NumeroDocumento,
    DateOnly FechaNacimiento,
    string Correo,
    string Celular,
    string? NombreResponsable,
    DateTime RegistradoEn);
