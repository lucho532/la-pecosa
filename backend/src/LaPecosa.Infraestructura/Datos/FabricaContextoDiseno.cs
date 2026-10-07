using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaPecosa.Infraestructura.Datos;

/// <summary>
/// Representa la fábrica que usan las herramientas de Entity Framework para crear migraciones.
/// Su responsabilidad es construir el contexto sin arrancar la API.
/// No se usa al ejecutar la aplicación ni se conecta a ninguna base de datos real.
/// </summary>
public class FabricaContextoDiseno : IDesignTimeDbContextFactory<ContextoLaPecosa>
{
    /// <inheritdoc />
    public ContextoLaPecosa CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<ContextoLaPecosa>()
            .UseNpgsql("Host=localhost;Database=lapecosa")
            .Options;

        return new ContextoLaPecosa(opciones, new ContextoClub());
    }
}
