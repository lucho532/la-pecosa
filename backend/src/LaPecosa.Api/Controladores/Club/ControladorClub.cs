using LaPecosa.Api.Autorizacion;
using LaPecosa.Api.Errores;
using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Dominio.Enumeraciones;
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
    private readonly IServicioConfiguracionClub _configuracion;

    /// <summary>Crea el controlador con sus servicios.</summary>
    public ControladorClub(IServicioConsultaClub consulta, IServicioConfiguracionClub configuracion)
    {
        _consulta = consulta;
        _configuracion = configuracion;
    }

    /// <summary>Datos e identidad del club elegido.</summary>
    [HttpGet]
    [IntegranteDelClub]
    [ProducesResponseType<ClubDto>(StatusCodes.Status200OK)]
    public Task<ClubDto> Obtener(CancellationToken cancelacion) => _consulta.ObtenerAsync(Integrante.Rol, cancelacion);

    /// <summary>El PRESIDENTE edita nombre, sede, dirección y contacto de su club.</summary>
    [HttpPut("configuracion")]
    [IntegranteDelClub(Rol.PRESIDENTE)]
    [ProducesResponseType<ClubDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<Problema>(StatusCodes.Status400BadRequest, Problema.TipoContenido)]
    [ProducesResponseType<Problema>(StatusCodes.Status409Conflict, Problema.TipoContenido)]
    public Task<ClubDto> ActualizarConfiguracion(ActualizarConfiguracionClubDto datos, CancellationToken cancelacion) =>
        _configuracion.ActualizarElPropioAsync(datos, Integrante.Rol, cancelacion);
}
