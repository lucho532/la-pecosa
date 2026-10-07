import { useEffect, useState } from 'react';
import { api } from '../api/cliente';
import { useSesion } from './useSesion';

interface FotoCargada {
  /** Versión de la foto a la que corresponde la dirección. */
  version: number;
  url: string;
}

/**
 * Foto de perfil de la cuenta con sesión, como dirección local lista para una etiqueta de imagen,
 * o `null` si no tiene. La foto es un dato personal: se pide a la API con la sesión y se muestra
 * desde memoria, nunca desde una dirección pública (research §16).
 */
export function useFotoPerfil(): string | null {
  const { sesion } = useSesion();
  const version = sesion?.versionFoto ?? 0;
  const [foto, setFoto] = useState<FotoCargada | null>(null);

  useEffect(() => {
    if (version === 0) {
      return;
    }

    let cancelado = false;
    let url: string | null = null;

    api
      .getArchivo('/api/cuenta/foto')
      .then((archivo) => {
        if (!cancelado) {
          url = URL.createObjectURL(archivo);
          setFoto({ version, url });
        }
      })
      .catch(() => {
        // Sin foto disponible se muestran las iniciales.
      });

    return () => {
      cancelado = true;
      if (url) {
        URL.revokeObjectURL(url);
      }
    };
  }, [version]);

  return foto && foto.version === version ? foto.url : null;
}
