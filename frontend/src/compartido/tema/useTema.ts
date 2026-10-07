import { useContext } from 'react';
import { ContextoTema, type ValorTema } from './contextoTema';

/** Tema actual de la interfaz y la acción para cambiarlo. */
export function useTema(): ValorTema {
  const valor = useContext(ContextoTema);
  if (!valor) {
    throw new Error('useTema debe usarse dentro de ProveedorTema.');
  }

  return valor;
}
