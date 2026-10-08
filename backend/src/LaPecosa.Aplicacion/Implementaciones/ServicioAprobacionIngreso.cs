using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que aprueba un ingreso en espera.
/// Su responsabilidad es comprobar con las reglas del dominio que quien aprueba puede hacerlo y
/// puede dejar ese rol, y aprobar con una única sentencia condicionada a que el ingreso siga en
/// espera: si no cambia ninguna fila, el ingreso ya estaba aprobado (409) o fue rechazado (404), y
/// nunca hay un segundo efecto (RF-026). Si la persona queda como JUGADOR, en la misma transacción
/// y con el club bloqueado la ubica en la categoría activa de su año de nacimiento; si el club no
/// la tiene, el ingreso se aprueba igual y queda sin categoría (RF-008 y RF-009 de la 003).
/// No asigna equipo, no genera cobros ni envía correos. No accede al contexto de Entity Framework
/// ni conoce HTTP.
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
        Guid usuarioRolId, AprobarIngresoDto datos, UsuarioRol quienAprueba, CancellationToken cancelacion = default)
    {
        var errores = new ErroresDeValidacion();
        if (datos.Rol is null || !Enum.IsDefined(datos.Rol.Value))
        {
            errores.Agregar("rol", "Elige cómo entra la persona: jugador, entrenador o directivo.");
        }

        errores.LanzarSiHayErrores();
        var rol = datos.Rol!.Value;

        if (!ReglaAprobacionIngreso.PuedeAprobar(quienAprueba.Rol, quienAprueba.Id, usuarioRolId))
        {
            throw new ExcepcionDeAplicacion("rol_no_autorizado", 403, "Tu rol en este club no permite hacer esto.");
        }

        if (!ReglaAprobacionIngreso.PuedeAsignar(quienAprueba.Rol, rol))
        {
            throw new ExcepcionDeAplicacion(
                "rol_no_asignable", 403, "No puedes asignar ese rol al aprobar un ingreso.");
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
                    rol,
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

                if (rol == Rol.JUGADOR)
                {
                    await _ubicador.UbicarAUnoAsync(integrante, cancelacion);
                }

                return MapperIngresos.AIngresoAprobado(integrante);
            },
            cancelacion);
    }
}
