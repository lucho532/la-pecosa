using LaPecosa.Api.Autorizacion;
using LaPecosa.Api.Errores;
using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LaPecosa.Api.Controladores.Plataforma;

/// <summary>
/// Representa los endpoints del panel de administración sobre las invitaciones de un club: invitar
/// a otro presidente y reenviar una invitación.
/// Su responsabilidad es recibir la petición y delegar en <see cref="IServicioInvitacionPresidente"/>.
/// No contiene reglas de negocio; solo admite a la cuenta DESARROLLADOR y nunca devuelve el token.
/// </summary>
[Route("api/plataforma/clubes/{clubId:guid}/invitaciones")]
[Tags("Plataforma")]
[SoloDesarrollador]
[ProducesResponseType<InvitacionDto>(StatusCodes.Status201Created)]
[ProducesResponseType<Problema>(StatusCodes.Status400BadRequest, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status401Unauthorized, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status403Forbidden, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status404NotFound, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status409Conflict, Problema.TipoContenido)]
public class ControladorInvitacionesClub : ControladorBase
{
    private readonly IServicioInvitacionPresidente _servicio;

    /// <summary>Crea el controlador con el servicio de invitaciones.</summary>
    public ControladorInvitacionesClub(IServicioInvitacionPresidente servicio)
    {
        _servicio = servicio;
    }

    /// <summary>Invita a un presidente a un club que ya existe.</summary>
    [HttpPost]
    public async Task<IActionResult> Invitar(Guid clubId, InvitarPresidenteDto datos, CancellationToken cancelacion) =>
        StatusCode(StatusCodes.Status201Created, await _servicio.InvitarAsync(clubId, datos, UsuarioId, cancelacion));

    /// <summary>Reenvía una invitación sin usar, con el mismo correo o con uno corregido.</summary>
    [HttpPost("{invitacionId:guid}/reenvio")]
    public async Task<IActionResult> Reenviar(
        Guid clubId,
        Guid invitacionId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReenviarInvitacionDto? datos,
        CancellationToken cancelacion) =>
        StatusCode(
            StatusCodes.Status201Created,
            await _servicio.ReenviarAsync(clubId, invitacionId, datos, UsuarioId, cancelacion));
}
