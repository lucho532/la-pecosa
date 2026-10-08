import { api } from '../../compartido/api/cliente';
import type { JugadorDeCategoriaDto, JugadorRetiradoDto } from '../../compartido/api/tiposCategorias';
import { useCarga } from '../../compartido/api/useCarga';
import { SeccionRetirados } from './SeccionRetirados';
import { SeccionSinCategoria } from './SeccionSinCategoria';

interface Props {
  clubId: string;
  esPresidente: boolean;
  /** Cambia cada vez que el apartado hace algo que puede mover jugadores; obliga a recargar. */
  version: number;
  /** Se llama cuando una de las dos listas cambió algo, para recargar el resto del apartado. */
  alCambiar: () => void;
}

/**
 * Las dos listas del club que solo ven el presidente y los directivos: "Sin categoría" y
 * "Retirados" (RF-031, RF-044). Van en un componente aparte para que a un entrenador, que no las
 * ve, ni siquiera se le pidan a la API.
 */
export function ListasDelClub({ clubId, esPresidente, version, alCambiar }: Props) {
  const base = `/api/clubes/${clubId}/jugadores`;
  const sinCategoria = useCarga(`${clubId}/sin-categoria/${version}`, () =>
    api.get<JugadorDeCategoriaDto[]>(`${base}/sin-categoria`),
  );
  const retirados = useCarga(`${clubId}/retirados/${version}`, () =>
    api.get<JugadorRetiradoDto[]>(`${base}/retirados`),
  );
  const comunes = { clubId, esPresidente, alCambiar };

  return (
    <>
      <SeccionSinCategoria {...comunes} sinCategoria={sinCategoria} />
      <SeccionRetirados {...comunes} retirados={retirados} />
    </>
  );
}
