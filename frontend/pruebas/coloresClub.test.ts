import { describe, expect, it } from 'vitest';
import { coloresClub } from '../src/compartido/tema/coloresClub';
import { TEXTO_CLARO, TEXTO_OSCURO } from '../src/compartido/tema/contraste';

describe('colores del club', () => {
  it('convierte la identidad en las variables del club, con el texto elegido por contraste', () => {
    expect(coloresClub({ colorPrincipal: '#B8370F', colorAcento: '#FFC72C', urlEscudo: null })).toEqual({
      '--color-club-principal': '#B8370F',
      '--color-club-principal-texto': TEXTO_CLARO,
      '--color-club-acento': '#FFC72C',
      '--color-club-acento-texto': TEXTO_OSCURO,
    });
  });

  it('con todos los campos nulos deja la identidad neutra de la plataforma', () => {
    expect(coloresClub({ colorPrincipal: null, colorAcento: null, urlEscudo: null })).toEqual({});
    expect(coloresClub(null)).toEqual({});
    expect(coloresClub(undefined)).toEqual({});
  });

  it('con un solo color deja el otro neutro', () => {
    expect(coloresClub({ colorPrincipal: '#1D4E89', colorAcento: null, urlEscudo: null })).toEqual({
      '--color-club-principal': '#1D4E89',
      '--color-club-principal-texto': TEXTO_CLARO,
    });
  });

  it('ignora un color con formato no válido en lugar de romper la interfaz', () => {
    expect(coloresClub({ colorPrincipal: 'rojo', colorAcento: '#FFF', urlEscudo: null })).toEqual({});
  });
});
