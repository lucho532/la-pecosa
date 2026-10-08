using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que retira y reincorpora jugadores.
/// Su responsabilidad es expresar el retiro con <c>Activo</c> (constitución §14.1), copiando en ese
/// momento el nombre de quien retira (§13), sacar al jugador de su categoría y de sus equipos, y,
/// al reincorporar, vaciar los datos del retiro (solo se guarda el estado actual, §18) y ubicarlo
/// con la misma regla que a un ingreso recién aprobado. Todo ocurre con el club bloqueado, así que
/// dos retiros simultáneos retiran una sola vez.
/// No accede al contexto de Entity Framework ni conoce HTTP. No impide el acceso del retirado: eso
/// lo hace la autorización en cada petición.
/// </summary>
public class ServicioRetiroJugador : IServicioRetiroJugador
{
    private readonly IRepositorioClub _club;
    private readonly IRepositorioJugadores _jugadores;
    private readonly UbicadorDeJugadores _ubicador;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioRetiroJugador(
        IRepositorioClub club,
        IRepositorioJugadores jugadores,
        UbicadorDeJugadores ubicador,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj)
    {
        _club = club;
        _jugadores = jugadores;
        _ubicador = ubicador;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    /// <inheritdoc />
    public Task RetirarAsync(Guid usuarioRolId, UsuarioRol quienRetira, CancellationToken cancelacion = default) =>
        _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                await _club.BloquearAsync(cancelacion);
                var integrante = await _jugadores.ObtenerAsync(usuarioRolId, cancelacion)
                    ?? throw ExcepcionDeAplicacion.NoEncontrado();

                // Ya estaba retirado: no hay un segundo efecto ni cambia quién lo retiró (RF-047).
                if (EstaRetirado(integrante))
                {
                    return;
                }

                if (!integrante.EsJugadorDelClub)
                {
                    throw ErroresDeCategorias.NoEsJugador();
                }

                await _jugadores.SacarDeSusEquiposAsync(usuarioRolId, cancelacion);
                await _jugadores.RetirarAsync(
                    usuarioRolId,
                    _reloj.AhoraUtc,
                    quienRetira.UsuarioId,
                    $"{quienRetira.Nombres} {quienRetira.Apellidos}",
                    cancelacion);
            },
            cancelacion);

    /// <inheritdoc />
    public Task<ReincorporacionDto> ReincorporarAsync(Guid usuarioRolId, CancellationToken cancelacion = default) =>
        _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                await _club.BloquearAsync(cancelacion);
                var integrante = await _jugadores.ObtenerAsync(usuarioRolId, cancelacion)
                    ?? throw ExcepcionDeAplicacion.NoEncontrado();

                if (!EstaRetirado(integrante) || !await _jugadores.ReincorporarAsync(usuarioRolId, cancelacion))
                {
                    throw ErroresDeCategorias.JugadorNoRetirado();
                }

                var categoria = await _ubicador.UbicarAUnoAsync(integrante, cancelacion);
                return new ReincorporacionDto(usuarioRolId, categoria?.Id, categoria?.Anio);
            },
            cancelacion);

    private static bool EstaRetirado(UsuarioRol integrante) => integrante.Rol == Rol.JUGADOR && !integrante.Activo;
}
