import { createContext } from 'react';
import type { SesionDto, TokenSesionDto } from '../api/tipos';

export interface ValorSesion {
  /** `cargando` mientras se comprueba el token guardado al abrir la aplicación. */
  estado: 'cargando' | 'sin_sesion' | 'con_sesion';
  sesion: SesionDto | null;
  /** Guarda el token recibido y carga la cuenta y sus clubes. */
  iniciar: (token: TokenSesionDto) => Promise<SesionDto>;
  /** Vuelve a pedir la cuenta y sus clubes a la API. */
  recargar: () => Promise<SesionDto | null>;
  /** Sustituye la sesión por la que devolvió la API (por ejemplo, al cambiar la foto). */
  actualizar: (sesion: SesionDto) => void;
  /** Borra el token del dispositivo. */
  cerrar: () => void;
}

export const ContextoSesion = createContext<ValorSesion | null>(null);
