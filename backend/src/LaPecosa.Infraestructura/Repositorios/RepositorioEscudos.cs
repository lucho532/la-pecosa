using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios;

/// <summary>
/// Representa la lectura del escudo con Entity Framework.
/// Su responsabilidad es leer el escudo del club de la petición. No filtra por club a mano: lo
/// hace el filtro global, que sin club en el contexto no devuelve ninguna fila.
/// No se salta el filtro de aislamiento ni guarda escudos.
/// </summary>
public class RepositorioEscudos : IRepositorioEscudos
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea el repositorio sobre el contexto de la petición.</summary>
    public RepositorioEscudos(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public Task<EscudoClub?> ObtenerAsync(CancellationToken cancelacion = default) =>
        _contexto.EscudosClub.AsNoTracking().FirstOrDefaultAsync(cancelacion);
}
