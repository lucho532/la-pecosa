import { describe, expect, it } from 'vitest';
import { esMenorDeEdad } from '../src/compartido/edad';

describe('menor de edad', () => {
  const hoy = new Date(2026, 9, 7);

  it('el día antes de cumplir 18 es menor', () => {
    expect(esMenorDeEdad('2008-10-08', hoy)).toBe(true);
  });

  it('el día de su cumpleaños 18 ya no es menor', () => {
    expect(esMenorDeEdad('2008-10-07', hoy)).toBe(false);
  });

  it('un niño es menor y un adulto no', () => {
    expect(esMenorDeEdad('2016-03-01', hoy)).toBe(true);
    expect(esMenorDeEdad('1988-03-15', hoy)).toBe(false);
  });

  it('quien nació un 29 de febrero cumple 18 el 1 de marzo de un año no bisiesto', () => {
    expect(esMenorDeEdad('2008-02-29', new Date(2026, 1, 28))).toBe(true);
    expect(esMenorDeEdad('2008-02-29', new Date(2026, 2, 1))).toBe(false);
  });

  it('sin fecha o con una fecha incompleta no marca a nadie como menor', () => {
    expect(esMenorDeEdad('', hoy)).toBe(false);
    expect(esMenorDeEdad('2016-03', hoy)).toBe(false);
  });
});
