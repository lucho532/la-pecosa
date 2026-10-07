import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ClubDeSesionDto } from '../src/compartido/api/tipos';
import { clubDeEntrada, guardarUltimoClub } from '../src/compartido/sesion/ultimoClub';

function club(clubId: string): ClubDeSesionDto {
  return {
    clubId,
    nombre: `Club ${clubId}`,
    rol: 'PRESIDENTE',
    estado: 'ACTIVO',
    identidad: { colorPrincipal: null, colorAcento: null, urlEscudo: null },
    nombres: 'Ana',
    apellidos: 'Pérez',
  };
}

describe('club de entrada', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it('sin clubes no hay club de entrada', () => {
    expect(clubDeEntrada([])).toBeNull();
  });

  it('quien tiene un solo club entra directamente a él', () => {
    expect(clubDeEntrada([club('a')])).toBe('a');
  });

  it('sin elección previa entra al primero de sus clubes', () => {
    expect(clubDeEntrada([club('a'), club('b')])).toBe('a');
  });

  it('recuerda el último club elegido', () => {
    guardarUltimoClub('b');

    expect(clubDeEntrada([club('a'), club('b')])).toBe('b');
  });

  it('si el último club ya no es suyo, vuelve a uno propio', () => {
    guardarUltimoClub('ajeno');

    expect(clubDeEntrada([club('a'), club('b')])).toBe('a');
  });

  it('funciona aunque el almacenamiento local no esté disponible', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('bloqueado');
    });
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('bloqueado');
    });

    expect(() => guardarUltimoClub('b')).not.toThrow();
    expect(clubDeEntrada([club('a'), club('b')])).toBe('a');
  });
});
