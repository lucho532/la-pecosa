using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa los casos de uso con los que el PRESIDENTE retira del club a un jugador que se fue
/// y lo reincorpora si vuelve (constitución §14.1; RF-041 a RF-047).
/// Su responsabilidad es dejar al jugador retirado, fuera de su categoría y de sus equipos y sin
/// acceso al club, conservando todos sus datos, y devolverlo al club ubicándolo como a un ingreso
/// recién aprobado.
/// No borra nada, no retira a quien no es un jugador aprobado, no envía correos y no comprueba el
/// rol de quien llama: lo hace la autorización.
/// </summary>
public interface IServicioRetiroJugador
{
    /// <summary>
    /// Retira al jugador. <paramref name="quienRetira"/> es el integrante de la sesión en ese
    /// club. Retirar a quien ya está retirado no tiene efecto.
    /// </summary>
    Task RetirarAsync(Guid usuarioRolId, UsuarioRol quienRetira, CancellationToken cancelacion = default);

    /// <summary>
    /// Reincorpora a un jugador retirado y lo ubica en la categoría activa de su año de nacimiento,
    /// si el club la tiene. No recupera sus equipos.
    /// </summary>
    Task<ReincorporacionDto> ReincorporarAsync(Guid usuarioRolId, CancellationToken cancelacion = default);
}
