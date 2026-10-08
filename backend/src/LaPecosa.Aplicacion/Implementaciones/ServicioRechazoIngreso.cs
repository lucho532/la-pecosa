using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que rechaza un ingreso en espera.
/// Su responsabilidad es borrar, en una sola transacción, al integrante (con una sentencia
/// condicionada a que siga en espera, para que entre aprobar y rechazar a la vez valga la primera
/// acción), las invitaciones del club enviadas a su correo y, si se quedó sin ningún club, su
/// cuenta. Es la eliminación física que justifica la constitución §14.
/// No rechaza a un integrante aprobado, no modifica los otros clubes de la persona, no envía
/// correos y no deja registro del rechazo. No accede al contexto de Entity Framework ni conoce HTTP.
/// </summary>
public class ServicioRechazoIngreso : IServicioRechazoIngreso
{
    private readonly IRepositorioIngresos _ingresos;
    private readonly IRepositorioInvitacionesClub _invitaciones;
    private readonly EliminadorDeCuentaSinClub _eliminador;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioRechazoIngreso(
        IRepositorioIngresos ingresos,
        IRepositorioInvitacionesClub invitaciones,
        EliminadorDeCuentaSinClub eliminador,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _ingresos = ingresos;
        _invitaciones = invitaciones;
        _eliminador = eliminador;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    /// <inheritdoc />
    public async Task RechazarAsync(Guid usuarioRolId, CancellationToken cancelacion = default)
    {
        var integrante = await _ingresos.ObtenerAsync(usuarioRolId, cancelacion)
            ?? throw ErroresDeIngreso.NoEncontrado();
        if (integrante.EstadoIngreso != EstadoIngreso.EN_ESPERA)
        {
            throw ErroresDeIngreso.YaAprobado();
        }

        var correo = integrante.Usuario?.CorreoNormalizado
            ?? throw new InvalidOperationException("El integrante debe venir con su cuenta cargada.");

        await _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                if (!await _ingresos.BorrarSiSigueEnEsperaAsync(usuarioRolId, cancelacion))
                {
                    // Otra persona se adelantó: si lo aprobó, sigue existiendo; si lo rechazó, ya no.
                    throw await _ingresos.ObtenerAsync(usuarioRolId, cancelacion) is null
                        ? ErroresDeIngreso.NoEncontrado()
                        : ErroresDeIngreso.YaAprobado();
                }

                await _invitaciones.BorrarDelCorreoAsync(correo, cancelacion);
                await _eliminador.EliminarSiQuedoSinClubAsync(integrante.UsuarioId, cancelacion);
            },
            cancelacion);
    }
}
