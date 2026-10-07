import { useCallback, useEffect, useState } from 'react';
import { mensajeDe } from './errores';

interface Carga<T> {
  datos: T | null;
  /** Mensaje del error de la última carga, o `null`. */
  error: string | null;
  /** El error completo, por si la pantalla necesita su código. */
  fallo: unknown;
  cargando: boolean;
  recargar: () => void;
  /** Sustituye los datos por los que devolvió la API tras una acción. */
  fijar: (datos: T) => void;
}

interface Resultado<T> {
  /** Carga a la que pertenece el resultado: la clave y el número de recarga. */
  de: string;
  datos: T | null;
  fallo: unknown;
}

/**
 * Carga datos de la API al montar el componente y cada vez que cambia `clave`.
 * `cargar` debe depender solo de `clave`. Al recargar se conservan los datos anteriores hasta que
 * llegan los nuevos; al cambiar de clave, no.
 */
export function useCarga<T>(clave: string, cargar: () => Promise<T>): Carga<T> {
  const [vuelta, setVuelta] = useState(0);
  const [resultado, setResultado] = useState<Resultado<T>>({ de: '', datos: null, fallo: null });
  const actual = `${clave}#${vuelta}`;

  useEffect(() => {
    let cancelado = false;
    cargar()
      .then((datos) => {
        if (!cancelado) {
          setResultado({ de: actual, datos, fallo: null });
        }
      })
      .catch((fallo: unknown) => {
        if (!cancelado) {
          setResultado({ de: actual, datos: null, fallo });
        }
      });

    return () => {
      cancelado = true;
    };
    // `cargar` se crea en cada pintado; la carga depende solo de la clave y de la vuelta.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [actual]);

  const recargar = useCallback(() => setVuelta((numero) => numero + 1), []);
  const fijar = useCallback(
    (datos: T) => setResultado((previo) => ({ ...previo, datos, fallo: null })),
    [],
  );

  const deEstaClave = resultado.de.startsWith(`${clave}#`);
  const fallo = deEstaClave ? resultado.fallo : null;

  return {
    datos: deEstaClave ? resultado.datos : null,
    error: fallo ? mensajeDe(fallo) : null,
    fallo,
    cargando: resultado.de !== actual,
    recargar,
    fijar,
  };
}
