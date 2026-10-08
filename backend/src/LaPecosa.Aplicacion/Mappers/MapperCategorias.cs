using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Mappers;

/// <summary>
/// Representa la conversión de categorías, equipos, asignaciones y jugadores en los DTO del
/// apartado "Categorías" y de la tarjeta "Mi categoría".
/// Su responsabilidad es que ninguna entidad salga por la API (constitución §5), que cada lista
/// salga ordenada y que de un jugador o de un entrenador solo salga lo que cada quien puede ver:
/// el club nunca recibe documento, correo ni celular (RF-032), y la familia recibe un tipo
/// distinto, sin identificadores (RF-035, §23).
/// No consulta la base de datos ni decide el alcance de quien pregunta. Solo muestra los equipos
/// activos y las asignaciones activas, aunque la categoría venga cargada con más.
/// </summary>
public static class MapperCategorias
{
    /// <summary>Categoría para la lista, con el número de jugadores que le corresponde.</summary>
    public static CategoriaDto ACategoria(Categoria categoria, int numeroJugadores) => new(
        categoria.Id,
        categoria.Anio,
        categoria.Activa,
        !categoria.Usada,
        numeroJugadores,
        Equipos(categoria),
        Entrenadores(categoria));

    /// <summary>Categoría con sus jugadores, que deben venir con sus equipos cargados.</summary>
    public static CategoriaDetalleDto ADetalle(Categoria categoria, IReadOnlyList<UsuarioRol> jugadores) => new(
        categoria.Id,
        categoria.Anio,
        categoria.Activa,
        !categoria.Usada,
        jugadores.Count,
        Equipos(categoria),
        Entrenadores(categoria),
        jugadores.PorApellidos().Select(jugador => AJugador(jugador, categoria)).ToList());

    /// <summary>
    /// Jugador de una lista. Con categoría, lleva sus equipos activos y la marca de fuera de su
    /// año (RF-016); sin ella, es un jugador de "Sin categoría".
    /// </summary>
    public static JugadorDeCategoriaDto AJugador(UsuarioRol jugador, Categoria? categoria) => new(
        jugador.Id,
        jugador.Nombres,
        jugador.Apellidos,
        jugador.FechaNacimiento.Year,
        categoria is not null && jugador.FechaNacimiento.Year != categoria.Anio,
        categoria is null ? [] : Referencias(categoria, jugador.Equipos.Select(fila => fila.EquipoId)));

    /// <summary>Integrante que se puede asignar como entrenador.</summary>
    public static CandidatoEntrenadorDto ACandidato(UsuarioRol integrante) =>
        new(integrante.Id, integrante.Nombres, integrante.Apellidos, integrante.Rol);

    /// <summary>
    /// Jugador retirado. Quién lo retiró sale de lo copiado al retirar, nunca del nombre actual (§13).
    /// </summary>
    public static JugadorRetiradoDto ARetirado(UsuarioRol jugador) => new(
        jugador.Id,
        jugador.Nombres,
        jugador.Apellidos,
        jugador.FechaNacimiento.Year,
        jugador.RetiradoPorNombre ?? string.Empty,
        jugador.RetiradoEn ?? throw new InvalidOperationException("Un jugador retirado debe tener la fecha de su retiro."));

    /// <summary>
    /// Lo que ve la familia: la categoría de su jugador, los nombres de sus equipos y, de cada
    /// entrenador, solo su nombre, sus apellidos y los equipos que dirige.
    /// </summary>
    public static MiCategoriaDto AMiCategoria(UsuarioRol jugador, Categoria? categoria)
    {
        if (categoria is null)
        {
            return new MiCategoriaDto(null);
        }

        var entrenadores = Entrenadores(categoria)
            .Select(entrenador => new EntrenadorParaFamiliaDto(
                entrenador.Nombres, entrenador.Apellidos, entrenador.Equipos.Select(equipo => equipo.Nombre).ToList()))
            .ToList();

        var equipos = Referencias(categoria, jugador.Equipos.Select(fila => fila.EquipoId))
            .Select(equipo => equipo.Nombre)
            .ToList();

        return new MiCategoriaDto(new CategoriaDeMiJugadorDto(categoria.Anio, equipos, entrenadores));
    }

    private static List<EquipoDto> Equipos(Categoria categoria) => categoria.Equipos
        .Where(equipo => equipo.Activo)
        .OrderBy(equipo => equipo.Nombre, OrdenDePersonas.Comparador)
        .Select(equipo => new EquipoDto(equipo.Id, equipo.Nombre, equipo.Jugadores.Count, !equipo.Usado))
        .ToList();

    private static List<EntrenadorDeCategoriaDto> Entrenadores(Categoria categoria) => categoria.Asignaciones
        .Where(asignacion => asignacion.Activa)
        .Select(asignacion => (
            Asignacion: asignacion,
            Integrante: asignacion.UsuarioRol
                ?? throw new InvalidOperationException("La asignación debe venir con su integrante cargado.")))
        .OrderBy(par => par.Integrante.Apellidos, OrdenDePersonas.Comparador)
        .ThenBy(par => par.Integrante.Nombres, OrdenDePersonas.Comparador)
        .Select(par => new EntrenadorDeCategoriaDto(
            par.Integrante.Id,
            par.Integrante.Nombres,
            par.Integrante.Apellidos,
            par.Integrante.Rol,
            Referencias(categoria, par.Asignacion.Equipos.Select(fila => fila.EquipoId))))
        .ToList();

    /// <summary>Los equipos activos de la categoría que están entre esos identificadores, por nombre.</summary>
    private static List<EquipoDeReferenciaDto> Referencias(Categoria categoria, IEnumerable<Guid> equipoIds)
    {
        var buscados = equipoIds.ToHashSet();
        return categoria.Equipos
            .Where(equipo => equipo.Activo && buscados.Contains(equipo.Id))
            .OrderBy(equipo => equipo.Nombre, OrdenDePersonas.Comparador)
            .Select(equipo => new EquipoDeReferenciaDto(equipo.Id, equipo.Nombre))
            .ToList();
    }
}
