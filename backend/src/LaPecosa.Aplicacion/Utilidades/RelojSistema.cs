using LaPecosa.Aplicacion.Interfaces;

namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa el reloj real del sistema.
/// Su responsabilidad es dar la fecha y hora actuales en UTC.
/// No aplica zonas horarias ni formatos.
/// </summary>
public class RelojSistema : IReloj
{
    /// <inheritdoc />
    public DateTime AhoraUtc => DateTime.UtcNow;
}
