import { createContext } from 'react';
import type { Tema } from './contraste';

export interface ValorTema {
  tema: Tema;
  /** Cambia al otro tema y recuerda la elección en el dispositivo. */
  alternar: () => void;
}

export const ContextoTema = createContext<ValorTema | null>(null);
