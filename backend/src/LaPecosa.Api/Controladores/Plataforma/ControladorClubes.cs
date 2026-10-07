using LaPecosa.Api.Autorizacion;
using LaPecosa.Api.Errores;
using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace LaPecosa.Api.Controladores.Plataforma;

/// <summary>
/// Representa los endpoints del panel de administración sobre los clubes: listar, crear y ver el
/// detalle.
/// Su responsabilidad es recibir la petición y delegar en los servicios.
/// No contiene reglas de negocio; solo admite a la cuenta DESARROLLADOR.
/// </summary>
[Route("api/plataforma/clubes")]
[Tags("Plataforma")]
[SoloDesarrollador]
[ProducesResponseType<Problema>(StatusCodes.Status401Unauthorized, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status403Forbidden, Problema.TipoContenido)]
public class ControladorClubes : ControladorBase
{
    private readonly IServicioConsultaClubes _consulta;
    private readonly IServicioCreacionClub _creacion;

    /// <summary>Crea el controlador con sus servicios.</summary>
    public ControladorClubes(IServicioConsultaClubes consulta, IServicioCreacionClub creacion)
    {
        _consulta = consulta;
        _creacion = creacion;
    }

    /// <summary>Lista de todos los clubes con su estado.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClubResumenDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ClubResumenDto>> Listar(CancellationToken cancelacion) =>
        _consulta.ListarAsync(cancelacion);

    /// <summary>Crea un club e invita a su presidente.</summary>
    [HttpPost]
    [ProducesResponseType<ClubDetalleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<Problema>(StatusCodes.Status400BadRequest, Problema.TipoContenido)]
    [ProducesResponseType<Problema>(StatusCodes.Status409Conflict, Problema.TipoContenido)]
    public async Task<IActionResult> Crear(CrearClubDto datos, CancellationToken cancelacion)
    {
        var club = await _creacion.CrearAsync(datos, UsuarioId, cancelacion);
        return CreatedAtAction(nameof(Obtener), new { clubId = club.ClubId }, club);
    }

    /// <summary>Detalle de un club, con sus presidentes y sus invitaciones sin usar.</summary>
    [HttpGet("{clubId:guid}")]
    [ProducesResponseType<ClubDetalleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Problema>(StatusCodes.Status404NotFound, Problema.TipoContenido)]
    public Task<ClubDetalleDto> Obtener(Guid clubId, CancellationToken cancelacion) =>
        _consulta.ObtenerDetalleAsync(clubId, cancelacion);
}
