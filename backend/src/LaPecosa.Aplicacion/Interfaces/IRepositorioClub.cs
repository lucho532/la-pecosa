using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el acceso al club de la petición, para sus propios integrantes.
/// Su responsabilidad es leer el club que la autorización fijó en <see cref="IContextoClub"/>.
/// No recibe un identificador de club ni permite leer ningún otro: sin club en el contexto no
/// devuelve nada.
/// </summary>
public interface IRepositorioClub
{
    /// <summary>El club de la petición, o nulo si no hay ninguno en el contexto.</summary>
    Task<Club?> ObtenerAsync(CancellationToken cancelacion = default);
}
