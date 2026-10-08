using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de consultas del apartado "Ingresos".
/// Su responsabilidad es leer la sala de espera y los ingresos aprobados del club de la petición y
/// convertirlos en DTO.
/// No modifica datos, no accede al contexto de Entity Framework y no recibe un identificador de
/// club: solo existe el que fijó la autorización.
/// </summary>
public class ServicioConsultaIngresos : IServicioConsultaIngresos
{
    private readonly IRepositorioIngresos _ingresos;

    /// <summary>Crea el servicio con el acceso a los ingresos del club.</summary>
    public ServicioConsultaIngresos(IRepositorioIngresos ingresos)
    {
        _ingresos = ingresos;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IngresoEnEsperaDto>> ListarEnEsperaAsync(CancellationToken cancelacion = default) =>
        (await _ingresos.ListarEnEsperaAsync(cancelacion)).Select(MapperIngresos.AIngresoEnEspera).ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<IngresoAprobadoDto>> ListarAprobadosAsync(CancellationToken cancelacion = default) =>
        (await _ingresos.ListarAprobadosAsync(cancelacion)).Select(MapperIngresos.AIngresoAprobado).ToList();
}
