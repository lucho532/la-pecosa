using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que aprueba un ingreso en espera.
/// Su responsabilidad es negar la aprobación que indique un rol distinto de JUGADOR y aprobar con
/// una única sentencia condicionada a que el ingreso siga en espera: si no cambia ninguna fila, el
/// ingreso ya estaba aprobado (409) o fue rechazado (404), y nunca hay un segundo efecto. La
/// persona queda siempre como JUGADOR (RF-016) y, en la misma transacción y con el club bloqueado,
/// en la categoría activa de su año de nacimiento; si el club no la tiene, el ingreso se aprueba
/// igual y queda sin categoría.
/// No deja elegir rol ni comprueba quién aprueba: solo llega aquí el PRESIDENTE, que nunca está en
/// espera. No asigna equipo, no genera cobros ni envía correos. No accede al contexto de Entity
/// Framework ni conoce HTTP.
/// </summary>
public class ServicioAprobacionIngreso : IServicioAprobacionIngreso
{
    private readonly IRepositorioIngresos _ingresos;
    private readonly IRepositorioClub _club;
    private readonly UbicadorDeJugadores _ubicador;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioAprobacionIngreso(
        IRepositorioIngresos ingresos,
        IRepositorioClub club,
        UbicadorDeJugadores ubicador,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj)
    {
        _ingresos = ingresos;
        _club = club;
        _ubicador = ubicador;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    /// <inheritdoc />
    public async Task<IngresoAprobadoDto> AprobarAsync(
        Guid usuarioRolId, AprobarIngresoDto? datos, UsuarioRol quienAprueba, CancellationToken cancelacion = default)
    {
        // Sin cuerpo, sin rol o con JUGADOR se aprueba; cualquier otro rol se niega (RF-016).
        if (datos?.Rol is { } rolIndicado && rolIndicado != Rol.JUGADOR)
        {
            throw ErroresDeIngreso.RolNoAsignable();
        }

        return await _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                // Con el club bloqueado, aprobar y crear la categoría del año a la vez deja siempre
                // al jugador dentro de ella (research §4 de la 003).
                await _club.BloquearAsync(cancelacion);

                // El nombre de quien aprueba se copia tal como es ahora (§13).
                var aprobado = await _ingresos.AprobarAsync(
                    usuarioRolId,
                    _reloj.AhoraUtc,
                    quienAprueba.UsuarioId,
                    $"{quienAprueba.Nombres} {quienAprueba.Apellidos}",
                    cancelacion);

                var integrante = await _ingresos.ObtenerAsync(usuarioRolId, cancelacion)
                    ?? throw ErroresDeIngreso.NoEncontrado();
                if (!aprobado)
                {
                    throw ErroresDeIngreso.YaAprobado();
                }

                await _ubicador.UbicarAUnoAsync(integrante, cancelacion);

                return MapperIngresos.AIngresoAprobado(integrante);
            },
            cancelacion);
    }
}
