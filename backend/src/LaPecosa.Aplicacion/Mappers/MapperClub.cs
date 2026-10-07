using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Mappers;

/// <summary>
/// Representa la conversión de un club en lo que ve uno de sus integrantes.
/// Su responsabilidad es entregar la configuración, la identidad y el rol de quien consulta.
/// No incluye presidentes, invitaciones ni quién cambió el estado: eso es del panel.
/// </summary>
public static class MapperClub
{
    /// <summary>Club tal como lo ve un integrante con ese rol.</summary>
    public static ClubDto AClub(Club club, Rol miRol) => new(
        club.Id,
        club.Nombre,
        club.Sede,
        club.Direccion,
        club.CorreoContacto,
        club.TelefonoContacto,
        club.Estado,
        MapperIdentidadClub.AIdentidad(club),
        miRol);
}
