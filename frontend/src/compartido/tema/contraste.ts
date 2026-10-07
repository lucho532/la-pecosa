// Contraste de color según WCAG 2.x. Los colores van siempre en formato #RRGGBB.

export type Tema = 'claro' | 'oscuro';

/** Fondo de cada tema; debe coincidir con `--fondo` de variables.css. */
export const FONDO_DEL_TEMA: Record<Tema, string> = {
  claro: '#F6F3EF',
  oscuro: '#1A1716',
};

/**
 * Colores de texto entre los que se elige sobre un relleno. Son blanco y negro puros a propósito:
 * con ellos, el mejor de los dos alcanza siempre al menos 4,58:1 sobre cualquier color, por encima
 * del 4,5:1 que pide WCAG para texto normal. Con un oscuro más suave no se garantiza.
 */
export const TEXTO_CLARO = '#FFFFFF';
export const TEXTO_OSCURO = '#000000';

/** Por debajo de este contraste con el fondo, un color de relleno deja de distinguirse (RF-009). */
export const CONTRASTE_MINIMO_DE_RELLENO = 3;

const FORMATO = /^#[0-9a-f]{6}$/i;

/** Indica si el texto es un color en formato #RRGGBB. */
export function esColor(color: string | null | undefined): color is string {
  return typeof color === 'string' && FORMATO.test(color);
}

function canal(valor: number): number {
  const proporcion = valor / 255;
  return proporcion <= 0.03928 ? proporcion / 12.92 : ((proporcion + 0.055) / 1.055) ** 2.4;
}

/** Luminancia relativa de un color, de 0 (negro) a 1 (blanco). */
export function luminancia(color: string): number {
  const rojo = parseInt(color.slice(1, 3), 16);
  const verde = parseInt(color.slice(3, 5), 16);
  const azul = parseInt(color.slice(5, 7), 16);
  return 0.2126 * canal(rojo) + 0.7152 * canal(verde) + 0.0722 * canal(azul);
}

/** Contraste entre dos colores, de 1 (iguales) a 21 (blanco sobre negro). */
export function contraste(uno: string, otro: string): number {
  const a = luminancia(uno);
  const b = luminancia(otro);
  return (Math.max(a, b) + 0.05) / (Math.min(a, b) + 0.05);
}

/** Color de texto, claro u oscuro, que mejor se lee sobre un color de relleno. */
export function textoSobre(relleno: string): string {
  return contraste(relleno, TEXTO_CLARO) >= contraste(relleno, TEXTO_OSCURO) ? TEXTO_CLARO : TEXTO_OSCURO;
}

/**
 * Temas en los que un color de relleno se pierde contra el fondo: su contraste con el fondo de ese
 * tema queda por debajo de 3:1. Sirve para avisar antes de guardar; no impide hacerlo.
 */
export function temasDondeSePierde(color: string): Tema[] {
  return (Object.keys(FONDO_DEL_TEMA) as Tema[]).filter(
    (tema) => contraste(color, FONDO_DEL_TEMA[tema]) < CONTRASTE_MINIMO_DE_RELLENO,
  );
}
