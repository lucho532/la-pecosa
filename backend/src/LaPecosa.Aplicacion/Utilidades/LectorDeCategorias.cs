using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;

namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa la lectura de una categoría tal como la devuelve la API, que comparten la consulta
/// y todas las operaciones que responden con la categoría después de cambiarla.
/// Su responsabilidad es cargar la categoría del club de la petición con sus equipos, sus
/// entrenadores y, en el detalle, sus jugadores, y convertirla en DTO.
/// No comprueba el alcance de quien pregunta ni cambia nada: eso lo hace cada servicio antes de
/// llamarlo.
/// </summary>
public class LectorDeCategorias
{
    private readonly IRepositorioCategorias _categorias;
    private readonly IRepositorioJugadores _jugadores;

    /// <summary>Crea el lector con sus dependencias.</summary>
    public LectorDeCategorias(IRepositorioCategorias categorias, IRepositorioJugadores jugadores)
    {
        _categorias = categorias;
        _jugadores = jugadores;
    }

    /// <summary>La categoría sin la lista de sus jugadores; 404 si no existe en el club.</summary>
    public async Task<CategoriaDto> ResumenAsync(Guid categoriaId, CancellationToken cancelacion = default)
    {
        var categoria = await _categorias.ObtenerAsync(categoriaId, cancelacion)
            ?? throw ExcepcionDeAplicacion.NoEncontrado();

        return MapperCategorias.ACategoria(categoria, await _categorias.ContarJugadoresAsync(categoriaId, cancelacion));
    }

    /// <summary>La categoría con sus jugadores; 404 si no existe en el club.</summary>
    public async Task<CategoriaDetalleDto> DetalleAsync(Guid categoriaId, CancellationToken cancelacion = default)
    {
        var categoria = await _categorias.ObtenerAsync(categoriaId, cancelacion)
            ?? throw ExcepcionDeAplicacion.NoEncontrado();

        return MapperCategorias.ADetalle(categoria, await _jugadores.ListarDeCategoriaAsync(categoriaId, cancelacion));
    }
}
