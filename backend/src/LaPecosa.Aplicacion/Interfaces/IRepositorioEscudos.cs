using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa la lectura del escudo del club de la petición.
/// Su responsabilidad es leer el escudo del club fijado en <see cref="IContextoClub"/>, con el
/// filtro de aislamiento activo: el escudo no es una excepción de §7.1.
/// No recibe un identificador de club ni guarda escudos (eso es del panel).
/// </summary>
public interface IRepositorioEscudos
{
    /// <summary>El escudo del club de la petición, o nulo si no tiene o no hay club en el contexto.</summary>
    Task<EscudoClub?> ObtenerAsync(CancellationToken cancelacion = default);
}
