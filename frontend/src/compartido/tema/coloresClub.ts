import type { IdentidadClubDto } from '../api/tipos';
import { esColor, textoSobre } from './contraste';

/** Variables CSS con las que un club pinta su identidad. */
export type VariablesDeClub = Partial<
  Record<
    | '--color-club-principal'
    | '--color-club-principal-texto'
    | '--color-club-acento'
    | '--color-club-acento-texto',
    string
  >
>;

/**
 * Convierte la identidad de un club en los valores de sus variables CSS. Los colores del club se
 * usan solo como relleno, y el texto sobre cada uno se elige por contraste, de modo que se lee en
 * los dos temas (research §11).
 *
 * Con campos nulos no devuelve nada para ese color: se queda la identidad neutra de la plataforma,
 * que variables.css define para cada tema.
 */
export function coloresClub(identidad: IdentidadClubDto | null | undefined): VariablesDeClub {
  const variables: VariablesDeClub = {};
  if (!identidad) {
    return variables;
  }

  if (esColor(identidad.colorPrincipal)) {
    variables['--color-club-principal'] = identidad.colorPrincipal;
    variables['--color-club-principal-texto'] = textoSobre(identidad.colorPrincipal);
  }

  if (esColor(identidad.colorAcento)) {
    variables['--color-club-acento'] = identidad.colorAcento;
    variables['--color-club-acento-texto'] = textoSobre(identidad.colorAcento);
  }

  return variables;
}
