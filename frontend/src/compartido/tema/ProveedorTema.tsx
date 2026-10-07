import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { ContextoTema, type ValorTema } from './contextoTema';
import type { Tema } from './contraste';
import { aplicarTema, elegirTema, temaActual, temaContrario, temaElegido } from './tema';

/**
 * Mantiene el tema de toda la aplicación. Arranca con el tema elegido o, si no hay elección, con
 * el del dispositivo, y mientras no se elija ninguno sigue al dispositivo si este cambia.
 */
export function ProveedorTema({ children }: { children: ReactNode }) {
  const [tema, setTema] = useState<Tema>(temaActual);

  useEffect(() => {
    aplicarTema(tema);
  }, [tema]);

  useEffect(() => {
    if (typeof window.matchMedia !== 'function') {
      return;
    }

    const consulta = window.matchMedia('(prefers-color-scheme: dark)');
    const alCambiar = () => {
      if (temaElegido() === null) {
        setTema(consulta.matches ? 'oscuro' : 'claro');
      }
    };

    consulta.addEventListener('change', alCambiar);
    return () => consulta.removeEventListener('change', alCambiar);
  }, []);

  const valor = useMemo<ValorTema>(
    () => ({
      tema,
      alternar: () => {
        const nuevo = temaContrario(tema);
        elegirTema(nuevo);
        setTema(nuevo);
      },
    }),
    [tema],
  );

  return <ContextoTema.Provider value={valor}>{children}</ContextoTema.Provider>;
}
