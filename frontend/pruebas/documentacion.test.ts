import { describe, expect, it } from 'vitest';
import { textoDeDocumentacion } from '../src/privado/categorias/textos';
import { motivoParaNoEnviar, TAMANO_MAXIMO_DE_ARCHIVO } from '../src/privado/ficha/archivos';

describe('estado de la documentación en las listas', () => {
  it('sin documentos pendientes está completa', () => {
    expect(textoDeDocumentacion(0)).toBe('Completa');
  });

  it('con uno pendiente lo dice en singular', () => {
    expect(textoDeDocumentacion(1)).toBe('Falta 1');
  });

  it('con dos pendientes lo dice en plural', () => {
    expect(textoDeDocumentacion(2)).toBe('Faltan 2');
  });
});

describe('aviso antes de enviar un archivo de la ficha', () => {
  it('admite un PDF y las tres imágenes dentro del tamaño', () => {
    for (const type of ['application/pdf', 'image/jpeg', 'image/png', 'image/webp']) {
      expect(motivoParaNoEnviar({ size: 1024, type })).toBeNull();
    }
  });

  it('admite exactamente 10 MB y avisa con un byte más', () => {
    expect(motivoParaNoEnviar({ size: TAMANO_MAXIMO_DE_ARCHIVO, type: 'application/pdf' })).toBeNull();
    expect(motivoParaNoEnviar({ size: TAMANO_MAXIMO_DE_ARCHIVO + 1, type: 'application/pdf' })).toContain('10 MB');
  });

  it('avisa de un formato que no es PDF ni imagen', () => {
    expect(motivoParaNoEnviar({ size: 1024, type: 'text/plain' })).toContain('PDF');
  });

  it('si el dispositivo no dice el tipo, deja que decida la API', () => {
    expect(motivoParaNoEnviar({ size: 1024, type: '' })).toBeNull();
  });
});
