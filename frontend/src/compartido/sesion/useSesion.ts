import { useContext } from 'react';
import { ContextoSesion, type ValorSesion } from './contextoSesion';

/** Sesión actual: la cuenta, sus clubes y las acciones para iniciarla y cerrarla. */
export function useSesion(): ValorSesion {
  const valor = useContext(ContextoSesion);
  if (!valor) {
    throw new Error('useSesion debe usarse dentro de ProveedorSesion.');
  }

  return valor;
}
