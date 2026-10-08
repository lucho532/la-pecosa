namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa a un jugador en una lista del club (RF-032).
/// Su responsabilidad es llevar su nombre, sus apellidos, su año de nacimiento, sus equipos y si
/// está en una categoría que no es la de su año (RF-016).
/// No lleva ningún otro dato suyo: ni documento, ni correo, ni celular.
/// </summary>
/// <param name="UsuarioRolId">Identificador del integrante en este club.</param>
/// <param name="Nombres">Nombres del jugador.</param>
/// <param name="Apellidos">Apellidos del jugador.</param>
/// <param name="AnioNacimiento">Año de nacimiento.</param>
/// <param name="FueraDeSuAnio">Verdadero si su categoría no es la de su año; falso en "Sin categoría".</param>
/// <param name="Equipos">Equipos de su categoría en los que juega.</param>
public record JugadorDeCategoriaDto(
    Guid UsuarioRolId,
    string Nombres,
    string Apellidos,
    int AnioNacimiento,
    bool FueraDeSuAnio,
    IReadOnlyList<EquipoDeReferenciaDto> Equipos);
