using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Mappers;

/// <summary>
/// Representa la conversión de invitaciones e integrantes en los DTO del apartado "Ingresos".
/// Su responsabilidad es que ninguna entidad salga por la API (constitución §5) y que cada DTO
/// lleve solo lo que el PRESIDENTE y los DIRECTIVOS pueden ver.
/// No incluye el token de las invitaciones ni su hash, y no consulta la base de datos.
/// </summary>
public static class MapperIngresos
{
    /// <summary>Invitación del club sin su token, con su estado en ese instante.</summary>
    public static InvitacionClubDto AInvitacionClub(Invitacion invitacion, string? enviadaPor, DateTime ahoraUtc) => new(
        invitacion.Id,
        invitacion.Correo,
        invitacion.EstadoEn(ahoraUtc),
        invitacion.EstadoEnvio,
        enviadaPor,
        invitacion.CreadaEn,
        invitacion.VenceEn);

    /// <summary>Persona de la sala de espera, a partir de un integrante con su cuenta cargada.</summary>
    public static IngresoEnEsperaDto AIngresoEnEspera(UsuarioRol integrante)
    {
        var cuenta = integrante.Usuario
            ?? throw new InvalidOperationException("El integrante en espera debe venir con su cuenta cargada.");

        return new IngresoEnEsperaDto(
            integrante.Id,
            integrante.Nombres,
            integrante.Apellidos,
            integrante.TipoDocumento,
            integrante.NumeroDocumento,
            integrante.FechaNacimiento,
            cuenta.Correo,
            cuenta.Celular ?? string.Empty,
            cuenta.NombreResponsable,
            integrante.CreadoEn);
    }

    /// <summary>
    /// Ingreso aprobado. Quién aprobó y con qué rol salen de lo copiado al aprobar, nunca del
    /// nombre ni del rol actuales (§13).
    /// </summary>
    public static IngresoAprobadoDto AIngresoAprobado(UsuarioRol integrante) => new(
        integrante.Id,
        integrante.Nombres,
        integrante.Apellidos,
        integrante.RolDeIngreso
            ?? throw new InvalidOperationException("Un ingreso aprobado debe tener su rol de ingreso."),
        integrante.AprobadoPorNombre ?? string.Empty,
        integrante.AprobadoEn
            ?? throw new InvalidOperationException("Un ingreso aprobado debe tener su fecha de aprobación."));
}
