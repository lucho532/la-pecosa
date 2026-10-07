import { useOutletContext } from 'react-router-dom';
import type { ClubDto } from '../compartido/api/tipos';

/** Lo que la disposición del club entrega a sus pantallas. */
export interface ContextoDelClub {
  club: ClubDto;
  /** Sustituye el club por el que devolvió la API tras guardar un cambio. */
  fijarClub: (club: ClubDto) => void;
}

/** Club elegido, ya cargado y comprobado por la API. Solo dentro de DisposicionClub. */
export function useClub(): ContextoDelClub {
  return useOutletContext<ContextoDelClub>();
}
