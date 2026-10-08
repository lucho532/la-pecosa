using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio que aprueba un ingreso en espera.
/// Su responsabilidad es comprobar con las reglas del dominio que quien aprueba puede hacerlo y
/// puede dejar ese rol, y aprobar con una única sentencia condicionada a que el ingreso siga en
/// espera: si no cambia ninguna fila, el ingreso ya estaba aprobado (409) o fue rechazado (404), y
/// nunca hay un segundo efecto (RF-026).
/// No asigna categoría, no genera cobros ni envía correos. No accede al contexto de Entity
/// Framework ni conoce HTTP.
/// </summary>
public class ServicioAprobacionIngreso : IServicioAprobacionIngreso
{
    private readonly IRepositorioIngresos _ingresos;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioAprobacionIngreso(IRepositorioIngresos ingresos, IReloj reloj)
    {
        _ingresos = ingresos;
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

        return aprobado ? MapperIngresos.AIngresoAprobado(integrante) : throw ErroresDeIngreso.YaAprobado();
    }
}
