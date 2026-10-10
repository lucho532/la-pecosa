namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa los errores del contrato propios de la ficha del jugador: los de sus archivos y los
/// del cambio de su documento de identidad.
/// Su responsabilidad es que todos los casos de uso de la ficha respondan con el mismo código y
/// con un texto que le diga a la persona qué puede hacer.
/// No decide cuándo se produce cada error ni lleva el de una ficha que no se puede ver, que es el
/// <c>no_encontrado</c> común.
/// </summary>
public static class ErroresDeFicha
{
    /// <summary>400: el archivo supera el tamaño máximo (RF-030).</summary>
    public static ExcepcionDeAplicacion ArchivoDemasiadoGrande() => new(
        "archivo_demasiado_grande", 400, "El archivo pesa más de 10 MB.");

    /// <summary>
    /// 400: el archivo no es de un formato admitido, o no se envió ninguno (RF-030).
    /// </summary>
    public static ExcepcionDeAplicacion ArchivoNoAdmitido() => new(
        "archivo_no_admitido", 400, "El archivo debe ser un PDF o una imagen JPEG, PNG o WebP.");
}
