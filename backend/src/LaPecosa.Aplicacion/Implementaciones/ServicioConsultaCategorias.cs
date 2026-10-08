using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de consultas del apartado "Categorías".
/// Su responsabilidad es leer las categorías del club de la petición y dejar pasar solo las que
/// <see cref="ReglaAlcanceDeCategorias"/> permite a quien pregunta: para un ENTRENADOR, las activas
/// en las que tiene una asignación activa, que se consulta en cada petición.
/// No modifica datos, no accede al contexto de Entity Framework y no recibe un identificador de
/// club: solo existe el que fijó la autorización.
/// </summary>
public class ServicioConsultaCategorias : IServicioConsultaCategorias
{
    private readonly IRepositorioCategorias _categorias;
    private readonly IRepositorioAsignaciones _asignaciones;
    private readonly IRepositorioJugadores _jugadores;
    private readonly LectorDeCategorias _lector;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioConsultaCategorias(
        IRepositorioCategorias categorias,
        IRepositorioAsignaciones asignaciones,
        IRepositorioJugadores jugadores,
        LectorDeCategorias lector)
    {
        _categorias = categorias;
        _asignaciones = asignaciones;
        _jugadores = jugadores;
        _lector = lector;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(
        UsuarioRol quienPregunta, CancellationToken cancelacion = default)
    {
        var categorias = await _categorias.ListarAsync(cancelacion);
        if (!ReglaAlcanceDeCategorias.VeTodas(quienPregunta.Rol))
        {
            var suyas = (await _asignaciones.CategoriasDeAsync(quienPregunta.Id, cancelacion)).ToHashSet();
            categorias = categorias
                .Where(categoria => ReglaAlcanceDeCategorias.PuedeVer(
                    quienPregunta.Rol, categoria.Activa, suyas.Contains(categoria.Id)))
                .ToList();
        }

        var jugadores = await _categorias.ContarJugadoresAsync(cancelacion);
        return categorias
            .Select(categoria => MapperCategorias.ACategoria(categoria, jugadores.GetValueOrDefault(categoria.Id)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<CategoriaDetalleDto> ObtenerAsync(
        Guid categoriaId, UsuarioRol quienPregunta, CancellationToken cancelacion = default)
    {
        if (!ReglaAlcanceDeCategorias.VeTodas(quienPregunta.Rol))
        {
            var categoria = await _categorias.ObtenerAsync(categoriaId, cancelacion);
            var asignada = categoria is not null
                && await _asignaciones.TieneActivaAsync(categoriaId, quienPregunta.Id, cancelacion);

            // El mismo 404 si no existe, si está inactiva o si no es suya: no se revela cuál (§17.2).
            if (categoria is null || !ReglaAlcanceDeCategorias.PuedeVer(quienPregunta.Rol, categoria.Activa, asignada))
            {
                throw ExcepcionDeAplicacion.NoEncontrado();
            }
        }

        return await _lector.DetalleAsync(categoriaId, cancelacion);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JugadorDeCategoriaDto>> ListarSinCategoriaAsync(CancellationToken cancelacion = default) =>
        (await _jugadores.ListarSinCategoriaAsync(cancelacion))
            .PorApellidos()
            .Select(jugador => MapperCategorias.AJugador(jugador, null))
            .ToList();

    /// <inheritdoc />
    public async Task<IReadOnlyList<JugadorRetiradoDto>> ListarRetiradosAsync(CancellationToken cancelacion = default) =>
        (await _jugadores.ListarRetiradosAsync(cancelacion)).Select(MapperCategorias.ARetirado).ToList();
}
