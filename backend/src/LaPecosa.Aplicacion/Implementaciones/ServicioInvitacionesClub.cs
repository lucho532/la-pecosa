using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Aplicacion.Validadores;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de las invitaciones que envía un club.
/// Su responsabilidad es crear, listar, reenviar y cancelar las invitaciones del club de la
/// petición. La invitación se guarda con el rol JUGADOR, que es con el que entra quien la use
/// (RF-003). El correo se intenta enviar después de confirmar la transacción, sin colas; si falla,
/// la invitación queda con el envío fallido y se puede reenviar (RF-008).
/// No guarda el token en claro, no accede al contexto de Entity Framework y no conoce HTTP. No
/// recibe un identificador de club: solo existe el que fijó la autorización.
/// </summary>
public class ServicioInvitacionesClub : IServicioInvitacionesClub
{
    private readonly IRepositorioClub _club;
    private readonly IRepositorioInvitacionesClub _invitaciones;
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IServicioCorreo _correo;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioInvitacionesClub(
        IRepositorioClub club,
        IRepositorioInvitacionesClub invitaciones,
        IRepositorioUsuarios usuarios,
        IUnidadDeTrabajo unidadDeTrabajo,
        IServicioCorreo correo,
        IReloj reloj)
    {
        _club = club;
        _invitaciones = invitaciones;
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _correo = correo;
        _reloj = reloj;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InvitacionClubDto>> ListarAsync(CancellationToken cancelacion = default)
    {
        var invitaciones = await _invitaciones.ListarLaMasRecientePorCorreoAsync(cancelacion);
        var nombres = await _invitaciones.NombresDeIntegrantesAsync(
            invitaciones.Select(invitacion => invitacion.CreadaPorUsuarioId).Distinct().ToList(), cancelacion);
        var ahora = _reloj.AhoraUtc;

        return invitaciones
            .Select(invitacion => MapperIngresos.AInvitacionClub(
                invitacion, nombres.GetValueOrDefault(invitacion.CreadaPorUsuarioId), ahora))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<InvitacionClubDto> InvitarAsync(
        InvitarAlClubDto datos, Guid usuarioId, CancellationToken cancelacion = default)
    {
        var club = await _club.ObtenerAsync(cancelacion) ?? throw ExcepcionDeAplicacion.NoEncontrado();

        var errores = new ErroresDeValidacion();
        ValidadorCorreo.Obligatorio(errores, "correo", datos.Correo);
        errores.LanzarSiHayErrores();

        var correo = NormalizadorTexto.Correo(datos.Correo);
        await ComprobarCorreoInvitableAsync(correo, cancelacion);

        return await CrearYEnviarAsync(club, correo, usuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<InvitacionClubDto> ReenviarAsync(
        Guid invitacionId, Guid usuarioId, CancellationToken cancelacion = default)
    {
        var club = await _club.ObtenerAsync(cancelacion) ?? throw ExcepcionDeAplicacion.NoEncontrado();
        var anterior = await PendienteAsync(invitacionId, cancelacion);
        await ComprobarCorreoInvitableAsync(anterior.Correo, cancelacion);

        return await CrearYEnviarAsync(club, anterior.Correo, usuarioId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<InvitacionClubDto> CancelarAsync(Guid invitacionId, CancellationToken cancelacion = default)
    {
        var invitacion = await PendienteAsync(invitacionId, cancelacion);
        invitacion.AnuladaEn = _reloj.AhoraUtc;
        await _unidadDeTrabajo.GuardarAsync(cancelacion);

        return await ADtoAsync(invitacion, cancelacion);
    }

    /// <summary>Solo se reenvía o cancela una invitación pendiente (RF-006).</summary>
    private async Task<Invitacion> PendienteAsync(Guid invitacionId, CancellationToken cancelacion)
    {
        var invitacion = await _invitaciones.ObtenerAsync(invitacionId, cancelacion)
            ?? throw ExcepcionDeAplicacion.NoEncontrado();

        if (invitacion.EstadoEn(_reloj.AhoraUtc) != EstadoInvitacion.PENDIENTE)
        {
            throw ExcepcionDeAplicacion.Conflicto(
                "invitacion_no_pendiente",
                "Esa invitación ya no está pendiente: ya se usó, venció o fue cancelada. Envía una nueva.");
        }

        return invitacion;
    }

    /// <summary>No se invita a quien ya está en el club o en su sala de espera, ni al DESARROLLADOR (RF-007).</summary>
    private async Task ComprobarCorreoInvitableAsync(string correo, CancellationToken cancelacion)
    {
        var desarrollador = await _usuarios.ObtenerDesarrolladorAsync(cancelacion);
        if (desarrollador?.CorreoNormalizado == correo)
        {
            throw ErroresDeInvitacion.CorreoDelDesarrollador();
        }

        switch (await _invitaciones.EstadoDeIngresoDelCorreoAsync(correo, cancelacion))
        {
            case EstadoIngreso.APROBADO:
                throw ExcepcionDeAplicacion.Conflicto(
                    "ya_esta_en_el_club", "Ese correo ya pertenece a un integrante de este club.");
            case EstadoIngreso.EN_ESPERA:
                throw ExcepcionDeAplicacion.Conflicto(
                    "ya_esta_en_espera",
                    "Esa persona ya se registró y está en la sala de espera. Aprueba su ingreso desde allí.");
        }
    }

    private async Task<InvitacionClubDto> CrearYEnviarAsync(
        Club club, string correo, Guid usuarioId, CancellationToken cancelacion)
    {
        var (invitacion, token) = await _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                var ahora = _reloj.AhoraUtc;
                await _invitaciones.AnularPendientesAsync(correo, ahora, cancelacion);

                var nueva = ConstructorInvitaciones.Nueva(club.Id, Rol.JUGADOR, correo, usuarioId, ahora);
                _invitaciones.Agregar(nueva.Invitacion);
                await _unidadDeTrabajo.GuardarAsync(cancelacion);
                return nueva;
            },
            cancelacion);

        var enviado = await _correo.EnviarInvitacionAsync(invitacion.Correo, club.Nombre, invitacion.Rol, token, cancelacion);
        invitacion.EstadoEnvio = enviado ? EstadoEnvio.ENVIADO : EstadoEnvio.FALLIDO;
        await _unidadDeTrabajo.GuardarAsync(cancelacion);

        return await ADtoAsync(invitacion, cancelacion);
    }

    private async Task<InvitacionClubDto> ADtoAsync(Invitacion invitacion, CancellationToken cancelacion)
    {
        var nombres = await _invitaciones.NombresDeIntegrantesAsync([invitacion.CreadaPorUsuarioId], cancelacion);
        return MapperIngresos.AInvitacionClub(
            invitacion, nombres.GetValueOrDefault(invitacion.CreadaPorUsuarioId), _reloj.AhoraUtc);
    }
}
