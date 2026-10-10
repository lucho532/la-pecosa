using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa a una persona de la sala de espera tal como la ve el PRESIDENTE de su club (RF-017).
/// Su responsabilidad es llevar sus datos, desde cuándo espera y de qué jugador del club es
/// hermano (RF-015 de la 006), para que el PRESIDENTE decida si aprueba o rechaza su ingreso.
/// No sale por ningún otro endpoint: es el único lugar donde la API entrega el nombre del
/// responsable. No lleva rol: quien está en espera es siempre JUGADOR y lo sigue siendo al
/// aprobarse (RF-016).
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
/// <param name="HermanoDe">
/// Nombres y apellidos del jugador desde cuya ficha se agregó; <c>null</c> si ese jugador ya no
/// existe o si el ingreso no viene de la ficha de un hermano.
/// </param>
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
    DateTime RegistradoEn,
    string? HermanoDe);
