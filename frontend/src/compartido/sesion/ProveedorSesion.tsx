import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { api } from '../api/cliente';
import type { SesionDto, TokenSesionDto } from '../api/tipos';
import { borrarToken, convieneRenovar, guardarToken, leerToken } from './almacenToken';
import { ContextoSesion, type ValorSesion } from './contextoSesion';

/**
 * Mantiene la sesión de toda la aplicación. Al abrirla, si hay un token guardado lo renueva
 * cuando le queda menos de la mitad de su vigencia y carga la cuenta con sus clubes.
 */
export function ProveedorSesion({ children }: { children: ReactNode }) {
  const [estado, setEstado] = useState<ValorSesion['estado']>(leerToken() ? 'cargando' : 'sin_sesion');
  const [sesion, setSesion] = useState<SesionDto | null>(null);

  const recargar = useCallback(async () => {
    if (!leerToken()) {
      setSesion(null);
      setEstado('sin_sesion');
      return null;
    }

    try {
      const actual = await api.get<SesionDto>('/api/sesion');
      setSesion(actual);
      setEstado('con_sesion');
      return actual;
    } catch {
      borrarToken();
      setSesion(null);
      setEstado('sin_sesion');
      return null;
    }
  }, []);

  useEffect(() => {
    let cancelado = false;

    async function abrir() {
      if (!leerToken()) {
        return;
      }

      if (convieneRenovar()) {
        try {
          guardarToken(await api.post<TokenSesionDto>('/api/sesion/renovacion'));
        } catch {
          // Si no se pudo renovar, el token actual sigue valiendo hasta que venza.
        }
      }

      if (!cancelado) {
        await recargar();
      }
    }

    void abrir();
    return () => {
      cancelado = true;
    };
  }, [recargar]);

  const valor = useMemo<ValorSesion>(
    () => ({
      estado,
      sesion,
      recargar,
      actualizar: setSesion,
      iniciar: async (token) => {
        guardarToken(token);
        const actual = await recargar();
        if (!actual) {
          throw new Error('No se pudo abrir la sesión.');
        }

        return actual;
      },
      cerrar: () => {
        borrarToken();
        setSesion(null);
        setEstado('sin_sesion');
      },
    }),
    [estado, sesion, recargar],
  );

  return <ContextoSesion.Provider value={valor}>{children}</ContextoSesion.Provider>;
}
