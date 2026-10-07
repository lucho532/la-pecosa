import { describe, expect, it } from 'vitest';
import { URL_API } from '../src/compartido/api/configuracion';

describe('configuración de la API', () => {
  it('no termina en barra', () => {
    expect(URL_API.endsWith('/')).toBe(false);
  });
});
