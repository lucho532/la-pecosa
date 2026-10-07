namespace LaPecosa.Api.Controladores;

/// <summary>
/// Representa la lectura de un archivo cargado en un formulario <c>multipart/form-data</c>.
/// Su responsabilidad es entregar sus bytes a los servicios, que son los que validan la imagen.
/// No valida el formato ni confía en el nombre o el tipo que declara el navegador.
/// </summary>
public static class ArchivoCargado
{
    /// <summary>
    /// Límite de la petición, por encima de 1 MB para que sea el validador de imágenes, y no el
    /// servidor, el que responda con el motivo cuando un archivo es demasiado grande.
    /// </summary>
    public const int LimiteDePeticion = 5 * 1024 * 1024;

    /// <summary>Bytes del archivo, o nulo si no se envió ninguno.</summary>
    public static async Task<byte[]?> LeerAsync(IFormFile? archivo, CancellationToken cancelacion)
    {
        if (archivo is null)
        {
            return null;
        }

        using var memoria = new MemoryStream();
        await archivo.CopyToAsync(memoria, cancelacion);
        return memoria.ToArray();
    }
}
