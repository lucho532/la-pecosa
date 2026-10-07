using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que consulta el club elegido.
/// Su responsabilidad es leer el club de la petición y convertirlo en DTO.
/// No recibe un identificador de club: solo existe el que fijó la autorización.
/// </summary>
public class ServicioConsultaClub : IServicioConsultaClub
{
    private readonly IRepositorioClub _club;

    /// <summary>Crea el servicio con el acceso al club de la petición.</summary>
    public ServicioConsultaClub(IRepositorioClub club)
    {
        _club = club;
    }

    /// <inheritdoc />
    public async Task<ClubDto> ObtenerAsync(Rol miRol, CancellationToken cancelacion = default)
    {
        var club = await _club.ObtenerAsync(cancelacion) ?? throw ExcepcionDeAplicacion.NoEncontrado();
        return MapperClub.AClub(club, miRol);
    }
}
