import { describe, expect, it } from 'vitest';
import {
  contraste,
  esColor,
  luminancia,
  temasDondeSePierde,
  TEXTO_CLARO,
  TEXTO_OSCURO,
  textoSobre,
} from '../src/compartido/tema/contraste';

describe('contraste WCAG', () => {
  it('blanco sobre negro da 21 y un color consigo mismo da 1', () => {
    expect(contraste('#FFFFFF', '#000000')).toBeCloseTo(21, 5);
    expect(contraste('#B8370F', '#B8370F')).toBeCloseTo(1, 5);
  });

  it('no depende del orden ni de las mayúsculas', () => {
    expect(contraste('#b8370f', '#FFFFFF')).toBeCloseTo(contraste('#FFFFFF', '#B8370F'), 10);
  });

  it('calcula la luminancia de los extremos y de un valor conocido', () => {
    expect(luminancia('#000000')).toBe(0);
    expect(luminancia('#FFFFFF')).toBeCloseTo(1, 5);
    // #777777 contra blanco es el ejemplo clásico de 4,48:1.
    expect(contraste('#777777', '#FFFFFF')).toBeCloseTo(4.48, 2);
  });
});

describe('texto sobre un color de relleno', () => {
  it('sobre un color oscuro elige texto claro', () => {
    expect(textoSobre('#6E2E24')).toBe(TEXTO_CLARO);
    expect(textoSobre('#231F1E')).toBe(TEXTO_CLARO);
  });

  it('sobre un color claro elige texto oscuro', () => {
    expect(textoSobre('#FFC72C')).toBe(TEXTO_OSCURO);
    expect(textoSobre('#FEFEFE')).toBe(TEXTO_OSCURO);
  });

  it('el texto elegido siempre se lee: al menos 4,5:1 en toda la gama', () => {
    for (let gris = 0; gris <= 255; gris++) {
      const par = gris.toString(16).padStart(2, '0');
      expect(contraste(`#${par}${par}${par}`, textoSobre(`#${par}${par}${par}`))).toBeGreaterThanOrEqual(4.5);
    }

    for (const color of ['#B8370F', '#1D4E89', '#0B6B43', '#FF0000', '#00FF00', '#0000FF', '#FF00FF', '#7F7F00']) {
      expect(contraste(color, textoSobre(color))).toBeGreaterThanOrEqual(4.5);
    }
  });
});

describe('aviso de contraste contra el fondo del tema (RF-009)', () => {
  it('un color casi blanco se pierde en el tema claro', () => {
    expect(temasDondeSePierde('#FEFEFE')).toEqual(['claro']);
  });

  it('un color casi negro se pierde en el tema oscuro', () => {
    expect(temasDondeSePierde('#1F1B1A')).toEqual(['oscuro']);
  });

  it('un color medio se distingue en los dos temas', () => {
    expect(temasDondeSePierde('#B8370F')).toEqual([]);
  });
});

describe('formato de color', () => {
  it('solo acepta #RRGGBB', () => {
    expect(esColor('#B8370F')).toBe(true);
    expect(esColor('#b8370f')).toBe(true);
    expect(esColor('#FFF')).toBe(false);
    expect(esColor('B8370F')).toBe(false);
    expect(esColor('rojo')).toBe(false);
    expect(esColor(null)).toBe(false);
  });
});
