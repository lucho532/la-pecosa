using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa la decisión de aprobar un ingreso (RF-022).
/// Su responsabilidad es llevar el rol con el que queda la persona: JUGADOR, ENTRENADOR o DIRECTIVO.
/// No lleva quién aprueba ni a quién: eso sale de la sesión y de la ruta. Admite cualquier valor
/// de la enumeración de roles para que un rol no asignable (PRESIDENTE, por ejemplo) se rechace
/// con su propio error en lugar de como un dato mal escrito.
/// </summary>
/// <param name="Rol">Rol elegido para la persona.</param>
public record AprobarIngresoDto(Rol? Rol);
