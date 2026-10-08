using LaPecosa.Aplicacion.DTOs;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa las consultas del apartado "Ingresos" de un club (RF-020 y RF-025a).
/// Su responsabilidad es entregar la sala de espera y la lista de ingresos aprobados del club de
/// la petición.
/// No modifica nada ni decide quién puede consultarlas: eso lo comprueba la autorización.
/// </summary>
public interface IServicioConsultaIngresos
{
    /// <summary>Personas en espera del club, de la más antigua a la más reciente.</summary>
    Task<IReadOnlyList<IngresoEnEsperaDto>> ListarEnEsperaAsync(CancellationToken cancelacion = default);

    /// <summary>Ingresos aprobados del club, del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<IngresoAprobadoDto>> ListarAprobadosAsync(CancellationToken cancelacion = default);
}
