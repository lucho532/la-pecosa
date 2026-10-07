import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';
import { contraste, FONDO_DEL_TEMA, type Tema } from '../src/compartido/tema/contraste';

/** Lee de variables.css los colores de un tema. */
function variablesDe(tema: Tema): Record<string, string> {
  const css = readFileSync(resolve(__dirname, '../src/compartido/tema/variables.css'), 'utf8');
  const bloque = css.split(`[data-tema='${tema}'] {`)[1].split('\n}')[0];
  const variables: Record<string, string> = {};
  for (const [, nombre, valor] of bloque.matchAll(/(--[\w-]+):\s*(#[0-9a-fA-F]{6});/g)) {
    variables[nombre] = valor;
  }

  return variables;
}

/** Parejas de texto y fondo que se usan juntas en la interfaz. */
const PAREJAS: [texto: string, fondo: string][] = [
  ['--texto', '--fondo'],
  ['--texto', '--superficie'],
  ['--texto', '--superficie-suave'],
  ['--texto-suave', '--fondo'],
  ['--texto-suave', '--superficie'],
  ['--enlace', '--fondo'],
  ['--enlace', '--superficie'],
  ['--peligro', '--superficie'],
  ['--peligro-texto', '--peligro'],
  ['--lateral-texto', '--lateral-fondo'],
  ['--lateral-texto', '--lateral-suave'],
  ['--lateral-texto-suave', '--lateral-fondo'],
  ['--lateral-texto-suave', '--lateral-suave'],
  ['--lateral-activo-texto', '--lateral-activo-fondo'],
  ['--exito-texto', '--exito-fondo'],
  ['--info-texto', '--info-fondo'],
  ['--aviso-texto', '--aviso-fondo'],
  ['--error-texto', '--error-fondo'],
  ['--neutro-texto', '--neutro-fondo'],
  ['--neutro-principal-texto', '--neutro-principal'],
  ['--neutro-acento-texto', '--neutro-acento'],
];

describe.each<Tema>(['claro', 'oscuro'])('tema %s', (tema) => {
  const variables = variablesDe(tema);

  it('define todos los colores que usa la interfaz', () => {
    for (const nombre of new Set(PAREJAS.flat())) {
      expect(variables[nombre], nombre).toMatch(/^#[0-9a-fA-F]{6}$/);
    }
  });

  it.each(PAREJAS)('%s se lee sobre %s (al menos 4,5:1)', (texto, fondo) => {
    expect(contraste(variables[texto], variables[fondo])).toBeGreaterThanOrEqual(4.5);
  });

  it('el borde de los campos se distingue del fondo (al menos 3:1)', () => {
    expect(contraste(variables['--borde-fuerte'], variables['--superficie'])).toBeGreaterThanOrEqual(3);
  });

  it('el fondo que usa el aviso de contraste es el mismo del tema', () => {
    expect(variables['--fondo'].toUpperCase()).toBe(FONDO_DEL_TEMA[tema]);
  });
});

describe('los dos temas', () => {
  it('definen exactamente las mismas variables', () => {
    expect(Object.keys(variablesDe('oscuro')).sort()).toEqual(Object.keys(variablesDe('claro')).sort());
  });
});
