using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Mappers;

/// <summary>
/// Representa la conversión de una cuenta y sus integrantes en los DTO de la sesión.
/// Su responsabilidad es que ninguna entidad salga por la API (constitución §5).
/// No incluye la contraseña ni su hash, y no consulta la base de datos.
/// </summary>
public static class MapperSesion
{
    /// <summary>Sesión de una cuenta con los clubes a los que pertenece.</summary>
    public static SesionDto ASesion(Usuario usuario, IEnumerable<UsuarioRol> integrantes) => new(
        usuario.Id,
        usuario.Correo,
        usuario.EsDesarrollador,
        usuario.VersionFoto,
        integrantes.Select(AClubDeSesion).ToList());

    /// <summary>Club del desplegable a partir de un integrante con su club cargado.</summary>
    public static ClubDeSesionDto AClubDeSesion(UsuarioRol integrante)
    {
        var club = integrante.Club
            ?? throw new InvalidOperationException("El integrante debe venir con su club cargado.");

        return new ClubDeSesionDto(
            club.Id,
            club.Nombre,
            integrante.Rol,
            club.Estado,
            integrante.EstadoIngreso,
            MapperIdentidadClub.AIdentidad(club),
            integrante.Nombres,
            integrante.Apellidos);
    }

    /// <summary>Token de sesión recién emitido.</summary>
    public static TokenSesionDto AToken(TokenEmitido token) => new(token.Token, token.VenceEn);
}
