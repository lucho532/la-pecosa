import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { mensajeDe } from '../../compartido/api/errores';
import type {
  CategoriaDetalleDto,
  CategoriaDto,
  JugadorDeCategoriaDto,
  UbicarJugadorDto,
} from '../../compartido/api/tiposCategorias';
import { nombreCompleto } from './textos';

/**
 * Estado del diálogo de ubicar o cambiar de categoría a un jugador, que comparten la lista "Sin
 * categoría" y los jugadores de una categoría: a quién se va a mover, el envío y su resultado.
 */
export function useUbicarJugador(clubId: string, alTerminar: (texto: string) => void) {
  const [jugador, setJugador] = useState<JugadorDeCategoriaDto | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | undefined>();

  function abrir(elegido: JugadorDeCategoriaDto) {
    setError(undefined);
    setJugador(elegido);
  }

  async function confirmar(destino: CategoriaDto) {
    if (!jugador) {
      return;
    }

    setEnviando(true);
    setError(undefined);
    try {
      const datos: UbicarJugadorDto = { categoriaId: destino.categoriaId };
      await api.put<CategoriaDetalleDto>(`/api/clubes/${clubId}/jugadores/${jugador.usuarioRolId}/categoria`, datos);
      setJugador(null);
      alTerminar(`${nombreCompleto(jugador)} quedó en la categoría ${destino.anio}.`);
    } catch (fallo) {
      setError(mensajeDe(fallo));
    } finally {
      setEnviando(false);
    }
  }

  return { jugador, enviando, error, abrir, confirmar, cerrar: () => setJugador(null) };
}
