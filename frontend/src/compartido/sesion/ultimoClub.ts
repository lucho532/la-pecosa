import type { ClubDeSesionDto } from '../api/tipos';

const CLAVE = 'lapecosa.ultimoClub';

/** Recuerda en el dispositivo el último club elegido. */
export function guardarUltimoClub(clubId: string): void {
  try {
    localStorage.setItem(CLAVE, clubId);
  } catch {
    // Sin almacenamiento disponible simplemente no se recuerda.
  }
}

function leerUltimoClub(): string | null {
  try {
    return localStorage.getItem(CLAVE);
  } catch {
    return null;
  }
}

/**
 * Club al que entra una persona: el último que eligió, si sigue siendo suyo; si no, el primero de
 * los suyos. Devuelve `null` si no pertenece a ninguno. Nunca devuelve un club ajeno.
 */
export function clubDeEntrada(clubes: ClubDeSesionDto[]): string | null {
  if (clubes.length === 0) {
    return null;
  }

  const ultimo = leerUltimoClub();
  return clubes.some((club) => club.clubId === ultimo) ? ultimo : clubes[0].clubId;
}
