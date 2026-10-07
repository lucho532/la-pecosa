using LaPecosa.Api.Autorizacion;
using LaPecosa.Api.Errores;
using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace LaPecosa.Api.Controladores.Club;

/// <summary>
/// Representa los endpoints del club elegido por uno de sus integrantes.
/// Su responsabilidad es recibir la petición y delegar en los servicios.
/// No contiene reglas de negocio: la pertenencia y el estado del club los comprueba
/// <see cref="IntegranteDelClubAttribute"/> en cada petición.
/// </summary>
[Route("api/clubes/{clubId:guid}")]
[Tags("Club")]
[ProducesResponseType<Problema>(StatusCodes.Status401Unauthorized, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status403Forbidden, Problema.TipoContenido)]
[ProducesResponseType<Problema>(StatusCodes.Status404NotFound, Problema.TipoContenido)]
public class ControladorClub : ControladorBase
{
    private readonly IServicioConsultaClub _consulta;

    /// <summary>Crea el controlador con sus servicios.</summary>
    public ControladorClub(IServicioConsultaClub consulta)
    {
        _consulta = consulta;
    }

    /// <summary>Datos e identidad del club elegido.</summary>
    [HttpGet]
    [IntegranteDelClub]
    [ProducesResponseType<ClubDto>(StatusCodes.Status200OK)]
    public Task<ClubDto> Obtener(CancellationToken cancelacion) => _consulta.ObtenerAsync(Integrante.Rol, cancelacion);
}
