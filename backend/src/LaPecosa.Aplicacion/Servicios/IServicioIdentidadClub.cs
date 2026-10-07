using LaPecosa.Aplicacion.DTOs;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de configurar la identidad visual de un club (constitución §7.2,
/// RF-008).
/// Su responsabilidad es guardar los colores y el escudo de un club desde el panel de
/// administración.
/// No lo usa ningún integrante del club: el escudo y los colores solo los cambia el DESARROLLADOR.
/// </summary>
public interface IServicioIdentidadClub
{
    /// <summary>Guarda los colores del club. Formato no válido: 400 <c>datos_invalidos</c>.</summary>
    Task<ClubDetalleDto> ActualizarColoresAsync(
        Guid clubId, ActualizarColoresDto datos, CancellationToken cancelacion = default);

    /// <summary>
    /// Guarda o reemplaza el escudo. Si no es PNG, JPEG ni WebP: 400 <c>escudo_no_es_imagen</c>; si
    /// pasa de 1 MB: 400 <c>escudo_demasiado_grande</c>. En ambos casos se conserva el anterior.
    /// </summary>
    Task<ClubDetalleDto> GuardarEscudoAsync(Guid clubId, byte[]? contenido, CancellationToken cancelacion = default);
}
