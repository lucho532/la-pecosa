using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el acceso a los jugadores del club de la petición: los integrantes con rol JUGADOR,
/// ingreso aprobado y no retirados, más los que el club retiró.
/// Su responsabilidad es leer las listas de jugadores y cambiar su categoría, su retiro y su
/// reincorporación con sentencias condicionadas, de modo que nunca se le asigne categoría a quien
/// no es jugador del club ni se mueva a quien ya tiene una cuando la ubicación es automática.
/// No recibe un identificador de club ni ve integrantes de otro club. No decide a qué categoría va
/// cada jugador ni bloquea el club: eso lo hacen los servicios.
/// </summary>
public interface IRepositorioJugadores
{
    /// <summary>Un integrante del club, con cualquier rol y estado, sin sus equipos; no lo sigue.</summary>
    Task<UsuarioRol?> ObtenerAsync(Guid usuarioRolId, CancellationToken cancelacion = default);

    /// <summary>Un integrante del club con los equipos en los que juega; no lo sigue.</summary>
    Task<UsuarioRol?> ObtenerConEquiposAsync(Guid usuarioRolId, CancellationToken cancelacion = default);

    /// <summary>Jugadores del club sin categoría, cada uno sin equipos.</summary>
    Task<IReadOnlyList<UsuarioRol>> ListarSinCategoriaAsync(CancellationToken cancelacion = default);

    /// <summary>Jugadores de una categoría, cada uno con los equipos en los que juega.</summary>
    Task<IReadOnlyList<UsuarioRol>> ListarDeCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default);

    /// <summary>Jugadores retirados del club, del retiro más reciente al más antiguo.</summary>
    Task<IReadOnlyList<UsuarioRol>> ListarRetiradosAsync(CancellationToken cancelacion = default);

    /// <summary>
    /// Ubicación automática de uno: pone a ese jugador en la categoría solo si es jugador del club
    /// y no tiene categoría. Devuelve si lo ubicó.
    /// </summary>
    Task<bool> UbicarSiNoTieneAsync(Guid usuarioRolId, Guid categoriaId, CancellationToken cancelacion = default);

    /// <summary>
    /// Ubicación automática de un año: pone en la categoría a todos los jugadores del club sin
    /// categoría nacidos ese año. Devuelve cuántos entraron.
    /// </summary>
    Task<int> RecogerDelAnioAsync(Guid categoriaId, int anio, CancellationToken cancelacion = default);

    /// <summary>
    /// Ubicación a mano: pone a ese jugador en la categoría, tenga o no otra, solo si es jugador
    /// del club. Devuelve si lo cambió.
    /// </summary>
    Task<bool> CambiarCategoriaAsync(Guid usuarioRolId, Guid categoriaId, CancellationToken cancelacion = default);

    /// <summary>Saca al integrante de todos los equipos en los que juega.</summary>
    Task SacarDeSusEquiposAsync(Guid usuarioRolId, CancellationToken cancelacion = default);

    /// <summary>
    /// Retira al jugador solo si es jugador del club: lo deja inactivo, sin categoría y con quién
    /// lo retiró y cuándo, los tres datos a la vez. Devuelve si lo retiró.
    /// </summary>
    Task<bool> RetirarAsync(
        Guid usuarioRolId,
        DateTime ahoraUtc,
        Guid retiradoPorUsuarioId,
        string retiradoPorNombre,
        CancellationToken cancelacion = default);

    /// <summary>
    /// Reincorpora al jugador solo si está retirado: lo deja activo y vacía a la vez los tres datos
    /// del retiro. Devuelve si lo reincorporó.
    /// </summary>
    Task<bool> ReincorporarAsync(Guid usuarioRolId, CancellationToken cancelacion = default);
}
