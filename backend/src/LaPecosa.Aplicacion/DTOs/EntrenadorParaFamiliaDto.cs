namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa lo único que una familia ve de un entrenador de la categoría de su jugador (RF-035).
/// Su responsabilidad es llevar su nombre, sus apellidos y los nombres de los equipos que dirige.
/// No lleva identificador, rol, correo, celular ni documento: es un tipo distinto del que ve el
/// club (constitución §23).
/// </summary>
/// <param name="Nombres">Nombres del entrenador.</param>
/// <param name="Apellidos">Apellidos del entrenador.</param>
/// <param name="Equipos">Nombres de los equipos que dirige; vacía si lo es de la categoría en general.</param>
public record EntrenadorParaFamiliaDto(string Nombres, string Apellidos, IReadOnlyList<string> Equipos);
