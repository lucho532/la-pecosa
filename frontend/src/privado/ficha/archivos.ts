/** Tamaño máximo de cada archivo de la ficha: 10 MB. El mismo límite que aplica la API. */
export const TAMANO_MAXIMO_DE_ARCHIVO = 10 * 1024 * 1024;

/** Formatos que admite la API para los documentos de la ficha. */
export const FORMATOS_ADMITIDOS = ['application/pdf', 'image/jpeg', 'image/png', 'image/webp'];

/** Lo que se le dice a la familia junto al botón de subir. */
export const AYUDA_DE_ARCHIVOS = 'Un archivo por documento: PDF o imagen JPEG, PNG o WebP, de hasta 10 MB.';

/**
 * Motivo para no enviar un archivo, o `null` si se puede enviar. Solo sirve para avisar pronto,
 * sin esperar a subirlo: quien decide es la API, que reconoce el archivo por su contenido. Si el
 * dispositivo no dice de qué tipo es, se envía y decide ella.
 */
export function motivoParaNoEnviar(archivo: { size: number; type: string }): string | null {
  if (archivo.size > TAMANO_MAXIMO_DE_ARCHIVO) {
    return 'El archivo pesa más de 10 MB. Elige uno más liviano.';
  }

  if (archivo.type !== '' && !FORMATOS_ADMITIDOS.includes(archivo.type)) {
    return 'El archivo debe ser un PDF o una imagen JPEG, PNG o WebP.';
  }

  return null;
}
