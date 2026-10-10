namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa el último cambio de una ficha (RF-038).
/// Su responsabilidad es decir cuándo fue, quién lo hizo, con el nombre que tenía en ese momento
/// (§13), y si fue la cuenta del propio jugador.
/// No es un historial: solo existe el último cambio, sin valores anteriores (§18).
/// </summary>
/// <param name="Fecha">Fecha y hora del cambio, en UTC.</param>
/// <param name="Autor">Nombre de quien cambió, tal como era en ese momento.</param>
/// <param name="PorLaCuentaDelJugador">Verdadero si lo hizo la cuenta del jugador (su familia) y no el PRESIDENTE.</param>
public record UltimoCambioDto(DateTime Fecha, string Autor, bool PorLaCuentaDelJugador);
