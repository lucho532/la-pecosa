import { useCallback, useState } from 'react';
import { mensajeDe } from '../../compartido/api/errores';
import type { Tono } from '../../compartido/componentes/Aviso';

export interface Mensaje {
  tono: Tono;
  texto: string;
}

/**
 * Ejecuta una acción contra la API y guarda su resultado como un mensaje para mostrar: el texto que
 * devuelve la acción si sale bien, o el mensaje del servidor si la rechaza. Evita repetir en cada
 * sección de "Categorías" el mismo manejo de "ocupado", éxito y error.
 */
export function useAccion() {
  const [ocupado, setOcupado] = useState(false);
  const [mensaje, setMensaje] = useState<Mensaje | null>(null);

  const ejecutar = useCallback(async (accion: () => Promise<string | null>): Promise<boolean> => {
    setOcupado(true);
    setMensaje(null);
    try {
      const texto = await accion();
      setMensaje(texto ? { tono: 'exito', texto } : null);
      return true;
    } catch (fallo) {
      setMensaje({ tono: 'error', texto: mensajeDe(fallo) });
      return false;
    } finally {
      setOcupado(false);
    }
  }, []);

  return { ocupado, mensaje, ejecutar, fijarMensaje: setMensaje };
}
