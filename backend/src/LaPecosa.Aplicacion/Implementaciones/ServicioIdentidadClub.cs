using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Aplicacion.Validadores;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de la identidad visual de un club.
/// Su responsabilidad es validar y guardar los colores y el escudo, y subir la versión del escudo
/// cada vez que cambia para que su dirección cambie y la caché se renueve.
/// No confía en la extensión ni en el tipo declarado del archivo, no accede al contexto de Entity
/// Framework y no conoce HTTP.
/// </summary>
public class ServicioIdentidadClub : IServicioIdentidadClub
{
    private readonly IRepositorioClubesPlataforma _clubes;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IServicioConsultaClubes _consulta;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioIdentidadClub(
        IRepositorioClubesPlataforma clubes, IUnidadDeTrabajo unidadDeTrabajo, IServicioConsultaClubes consulta)
    {
        _clubes = clubes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _consulta = consulta;
    }

    /// <inheritdoc />
    public async Task<ClubDetalleDto> ActualizarColoresAsync(
        Guid clubId, ActualizarColoresDto datos, CancellationToken cancelacion = default)
    {
        var club = await _clubes.ObtenerAsync(clubId, cancelacion) ?? throw ExcepcionDeAplicacion.NoEncontrado();
        ValidadorColores.Validar(datos);

        club.ColorPrincipal = datos.ColorPrincipal!.ToUpperInvariant();
        club.ColorAcento = datos.ColorAcento!.ToUpperInvariant();
        await _unidadDeTrabajo.GuardarAsync(cancelacion);

        return await _consulta.ObtenerDetalleAsync(clubId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<ClubDetalleDto> GuardarEscudoAsync(
        Guid clubId, byte[]? contenido, CancellationToken cancelacion = default)
    {
        var club = await _clubes.ObtenerAsync(clubId, cancelacion) ?? throw ExcepcionDeAplicacion.NoEncontrado();

        var (tipoContenido, motivo) = ValidadorImagen.Validar(contenido ?? []);
        if (tipoContenido is null)
        {
            throw motivo == MotivoRechazoImagen.DemasiadoGrande
                ? new ExcepcionDeAplicacion("escudo_demasiado_grande", 400, "El escudo pesa más de 1 MB. Usa una imagen más liviana.")
                : new ExcepcionDeAplicacion("escudo_no_es_imagen", 400, "El escudo debe ser una imagen PNG, JPEG o WebP.");
        }

        await _unidadDeTrabajo.EnTransaccionAsync(
            async () =>
            {
                await _clubes.GuardarEscudoAsync(clubId, contenido!, tipoContenido, cancelacion);
                club.VersionEscudo++;
                await _unidadDeTrabajo.GuardarAsync(cancelacion);
            },
            cancelacion);

        return await _consulta.ObtenerDetalleAsync(clubId, cancelacion);
    }
}
