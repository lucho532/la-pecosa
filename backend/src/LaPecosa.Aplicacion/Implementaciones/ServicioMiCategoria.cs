using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de la tarjeta "Mi categoría" de la familia.
/// Su responsabilidad es leer el integrante de quien pregunta con sus equipos y la categoría que
/// tiene, y convertirlos en el DTO de la familia, que es un tipo distinto del que ve el club y no
/// lleva identificadores (constitución §23).
/// No lee ninguna otra categoría ni ningún otro jugador, no modifica datos y no accede al contexto
/// de Entity Framework.
/// </summary>
public class ServicioMiCategoria : IServicioMiCategoria
{
    private readonly IRepositorioJugadores _jugadores;
    private readonly IRepositorioCategorias _categorias;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioMiCategoria(IRepositorioJugadores jugadores, IRepositorioCategorias categorias)
    {
        _jugadores = jugadores;
        _categorias = categorias;
    }

    /// <inheritdoc />
    public async Task<MiCategoriaDto> ObtenerAsync(UsuarioRol quienPregunta, CancellationToken cancelacion = default)
    {
        var jugador = await _jugadores.ObtenerConEquiposAsync(quienPregunta.Id, cancelacion)
            ?? throw ExcepcionDeAplicacion.NoEncontrado();

        var categoria = jugador.CategoriaId is { } categoriaId
            ? await _categorias.ObtenerAsync(categoriaId, cancelacion)
            : null;

        return MapperCategorias.AMiCategoria(jugador, categoria);
    }
}
