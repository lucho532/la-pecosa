using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que acepta una invitación con una cuenta existente.
/// Su responsabilidad es comprobar que la invitación está vigente y es del correo de la sesión, y
/// en una transacción marcarla como usada y crear el integrante (copiando la identidad del
/// integrante más reciente de la cuenta) o reemplazar el rol del que ya tenía en ese club.
/// No crea cuentas ni pide de nuevo los datos de la persona. No accede al contexto de Entity
/// Framework ni conoce HTTP.
/// </summary>
public class ServicioAceptacionInvitacion : IServicioAceptacionInvitacion
{
    private readonly IRepositorioInvitacionesPorToken _invitaciones;
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioPertenencias _pertenencias;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioAceptacionInvitacion(
        IRepositorioInvitacionesPorToken invitaciones,
        IRepositorioUsuarios usuarios,
        IRepositorioPertenencias pertenencias,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj)
    {
        _invitaciones = invitaciones;
        _usuarios = usuarios;
        _pertenencias = pertenencias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    /// <inheritdoc />
    public async Task<(ClubDeSesionDto Club, bool Creada)> AceptarAsync(
        TokenDto datos, Guid usuarioId, CancellationToken cancelacion = default)
    {
        var ahora = _reloj.AhoraUtc;
        var invitacion = string.IsNullOrWhiteSpace(datos.Token)
            ? null
            : await _invitaciones.ObtenerPorHashAsync(GeneradorTokens.Hash(datos.Token), cancelacion);
        if (invitacion?.Club is null || !invitacion.EstaVigente(ahora))
        {
            throw ErroresDeInvitacion.NoValida();
        }

        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, cancelacion);
        if (usuario is null || usuario.CorreoNormalizado != invitacion.Correo)
        {
            throw new ExcepcionDeAplicacion(
                "invitacion_de_otro_correo",
                403,
                "Esta invitación se envió a otro correo. Cierra sesión y entra con la cuenta de ese correo.");
        }

        var existente = await _pertenencias.ObtenerAsync(usuarioId, invitacion.ClubId, cancelacion);
        var integrante = existente ?? await NuevoIntegranteAsync(usuario, invitacion, ahora, cancelacion);

        try
        {
            await _unidadDeTrabajo.EnTransaccionAsync(
                async () =>
                {
                    if (!await _invitaciones.MarcarUsadaAsync(invitacion.Id, ahora, cancelacion))
                    {
                        throw ErroresDeInvitacion.NoValida();
                    }

                    if (existente is null)
                    {
                        _pertenencias.Agregar(integrante);
                    }
                    else
                    {
                        // Ya era integrante de este club: su rol pasa a ser el de la invitación (RF-018).
                        existente.Rol = invitacion.Rol;
                        existente.EstadoIngreso = EstadoIngreso.APROBADO;
                    }

                    await _unidadDeTrabajo.GuardarAsync(cancelacion);
                },
                cancelacion);
        }
        catch (Exception error) when (_unidadDeTrabajo.EsViolacionDeUnicidad(error, out var indice)
            && ErroresDeInvitacion.DeIndiceUnico(indice) is { } conflicto)
        {
            throw conflicto;
        }

        integrante.Club = invitacion.Club;
        return (MapperSesion.AClubDeSesion(integrante), existente is null);
    }

    private async Task<UsuarioRol> NuevoIntegranteAsync(
        Usuario usuario, Invitacion invitacion, DateTime ahora, CancellationToken cancelacion)
    {
        // La persona es la misma en todos sus clubes: se copia su identidad del integrante más
        // reciente de la cuenta (research §7).
        var identidad = (await _pertenencias.ListarDeUsuarioAsync(usuario.Id, cancelacion))
            .OrderByDescending(integrante => integrante.CreadoEn)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Una cuenta con invitación debe tener algún integrante.");

        return new UsuarioRol
        {
            ClubId = invitacion.ClubId,
            UsuarioId = usuario.Id,
            Rol = invitacion.Rol,
            EstadoIngreso = EstadoIngreso.APROBADO,
            Nombres = identidad.Nombres,
            Apellidos = identidad.Apellidos,
            TipoDocumento = identidad.TipoDocumento,
            NumeroDocumento = identidad.NumeroDocumento,
            FechaNacimiento = identidad.FechaNacimiento,
            CreadoEn = ahora,
        };
    }
}
