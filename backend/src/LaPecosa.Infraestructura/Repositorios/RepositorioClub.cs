using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Infraestructura.Repositorios;

/// <summary>
/// Representa el acceso al club de la petición con Entity Framework.
/// Su responsabilidad es leer únicamente el club fijado en <see cref="IContextoClub"/>.
/// No lee ningún otro club: sin club en el contexto devuelve nulo (falla cerrado).
/// </summary>
public class RepositorioClub : IRepositorioClub
{
    private readonly ContextoLaPecosa _contexto;
    private readonly IContextoClub _contextoClub;

    /// <summary>Crea el repositorio sobre el contexto de datos y el club de la petición.</summary>
    public RepositorioClub(ContextoLaPecosa contexto, IContextoClub contextoClub)
    {
        _contexto = contexto;
        _contextoClub = contextoClub;
    }

    /// <inheritdoc />
    public async Task<Club?> ObtenerAsync(CancellationToken cancelacion = default)
    {
        var clubId = _contextoClub.ClubId;
        return clubId is null
            ? null
            : await _contexto.Clubes.FirstOrDefaultAsync(club => club.Id == clubId, cancelacion);
    }
}
