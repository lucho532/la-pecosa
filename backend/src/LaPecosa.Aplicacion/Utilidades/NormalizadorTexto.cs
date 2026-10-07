using System.Text.RegularExpressions;

namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa el único lugar donde se normalizan los textos que identifican algo: correo, nombre
/// de club, documento e identificador de inicio de sesión (RF-002).
/// Su responsabilidad es que lo guardado y lo escrito al buscar coincidan siempre.
/// No normaliza contraseñas (recortarlas o pasarlas a minúsculas las debilitaría) ni valida formatos.
/// </summary>
public static partial class NormalizadorTexto
{
    /// <summary>Correo sin espacios en los extremos y en minúsculas.</summary>
    public static string Correo(string? correo) => (correo ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>Nombre de club en minúsculas, sin espacios en los extremos ni repetidos.</summary>
    public static string NombreClub(string? nombre) => SinEspaciosSobrantes(nombre).ToLowerInvariant();

    /// <summary>Texto para mostrar: sin espacios en los extremos ni repetidos, con sus mayúsculas.</summary>
    public static string SinEspaciosSobrantes(string? texto) =>
        Espacios().Replace((texto ?? string.Empty).Trim(), " ");

    /// <summary>Número de documento sin espacios ni puntos y en minúsculas.</summary>
    public static string Documento(string? documento) =>
        EspaciosYPuntos().Replace(documento ?? string.Empty, string.Empty).ToLowerInvariant();

    /// <summary>Identificador de inicio de sesión sin espacios en los extremos y en minúsculas.</summary>
    public static string Identificador(string? identificador) =>
        (identificador ?? string.Empty).Trim().ToLowerInvariant();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacios();

    [GeneratedRegex(@"[\s.]+")]
    private static partial Regex EspaciosYPuntos();
}
