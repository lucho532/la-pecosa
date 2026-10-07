import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  aplicarTema,
  CLAVE_TEMA,
  elegirTema,
  temaActual,
  temaContrario,
  temaElegido,
} from '../src/compartido/tema/tema';

function dispositivo(oscuro: boolean) {
  vi.stubGlobal(
    'matchMedia',
    vi.fn((consulta: string) => ({ matches: oscuro && consulta.includes('dark'), media: consulta })),
  );
}

describe('tema claro y oscuro', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
    localStorage.clear();
    document.documentElement.removeAttribute('data-tema');
  });

  it('con una elección guardada manda la elección, no el dispositivo', () => {
    dispositivo(true);
    localStorage.setItem(CLAVE_TEMA, 'claro');

    expect(temaElegido()).toBe('claro');
    expect(temaActual()).toBe('claro');
  });

  it('sin elección y con el dispositivo en claro abre en claro', () => {
    dispositivo(false);

    expect(temaElegido()).toBeNull();
    expect(temaActual()).toBe('claro');
  });

  it('sin elección y con el dispositivo en oscuro abre en oscuro', () => {
    dispositivo(true);

    expect(temaActual()).toBe('oscuro');
  });

  it('elegir un tema lo guarda en el dispositivo y lo aplica a toda la interfaz', () => {
    dispositivo(false);

    elegirTema('oscuro');

    expect(localStorage.getItem(CLAVE_TEMA)).toBe('oscuro');
    expect(document.documentElement.getAttribute('data-tema')).toBe('oscuro');
    expect(temaActual()).toBe('oscuro');
  });

  it('un valor guardado que no es un tema se ignora', () => {
    dispositivo(true);
    localStorage.setItem(CLAVE_TEMA, 'fucsia');

    expect(temaElegido()).toBeNull();
    expect(temaActual()).toBe('oscuro');
  });

  it('sin almacenamiento disponible usa el dispositivo y aun así aplica el tema elegido', () => {
    dispositivo(true);
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('bloqueado');
    });
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('bloqueado');
    });

    expect(temaActual()).toBe('oscuro');
    expect(() => elegirTema('claro')).not.toThrow();
    expect(document.documentElement.getAttribute('data-tema')).toBe('claro');
  });

  it('sin matchMedia abre en claro', () => {
    vi.stubGlobal('matchMedia', undefined);

    expect(temaActual()).toBe('claro');
  });

  it('aplica el tema y sabe cuál es el contrario', () => {
    aplicarTema('oscuro');

    expect(document.documentElement.getAttribute('data-tema')).toBe('oscuro');
    expect(temaContrario('oscuro')).toBe('claro');
    expect(temaContrario('claro')).toBe('oscuro');
  });
});
