using LaPecosa.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LaPecosa.Infraestructura.Datos;

/// <summary>
/// Representa la unidad de trabajo sobre el contexto de Entity Framework.
/// Su responsabilidad es guardar los cambios y agrupar operaciones en una transacción.
/// No contiene consultas ni reglas de negocio.
/// </summary>
public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly ContextoLaPecosa _contexto;

    /// <summary>Crea la unidad de trabajo sobre el contexto de la petición.</summary>
    public UnidadDeTrabajo(ContextoLaPecosa contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public Task GuardarAsync(CancellationToken cancelacion = default) => _contexto.SaveChangesAsync(cancelacion);

    /// <inheritdoc />
    public async Task<T> EnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken cancelacion = default)
    {
        if (_contexto.Database.CurrentTransaction is not null)
        {
            return await operacion();
        }

        await using var transaccion = await _contexto.Database.BeginTransactionAsync(cancelacion);
        try
        {
            var resultado = await operacion();
            await transaccion.CommitAsync(cancelacion);
            return resultado;
        }
        catch
        {
            // Lo que quedó a medias en memoria no debe guardarse en una operación posterior.
            _contexto.ChangeTracker.Clear();
            throw;
        }
    }

    /// <inheritdoc />
    public Task EnTransaccionAsync(Func<Task> operacion, CancellationToken cancelacion = default) =>
        EnTransaccionAsync(
            async () =>
            {
                await operacion();
                return true;
            },
            cancelacion);

    /// <inheritdoc />
    public bool EsViolacionDeUnicidad(Exception error, out string? nombreIndice)
    {
        nombreIndice = null;
        if (error is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres })
        {
            nombreIndice = postgres.ConstraintName;
            return true;
        }

        return false;
    }
}
