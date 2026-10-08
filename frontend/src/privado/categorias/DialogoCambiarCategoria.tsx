import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import type { CategoriaDto, JugadorDeCategoriaDto } from '../../compartido/api/tiposCategorias';
import { useCarga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Campo } from '../../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { nombreCompleto } from './textos';

interface Props {
  clubId: string;
  jugador: JugadorDeCategoriaDto;
  /** Categoría en la que está hoy; no se ofrece como destino. Sin ella, el jugador no tiene categoría. */
  categoriaActualId?: string;
  cargando: boolean;
  error?: string;
  alConfirmar: (destino: CategoriaDto) => void;
  alCancelar: () => void;
}

/**
 * Diálogo para ubicar a un jugador sin categoría o pasarlo de una categoría a otra. Ofrece las
 * categorías activas del club y avisa cuando la elegida no es la de su año de nacimiento, que el
 * presidente puede elegir igualmente (RF-014). Confirmar en el diálogo es la confirmación.
 */
export function DialogoCambiarCategoria({
  clubId,
  jugador,
  categoriaActualId,
  cargando,
  error,
  alConfirmar,
  alCancelar,
}: Props) {
  const [elegidaId, setElegidaId] = useState('');
  const categorias = useCarga(`${clubId}/categorias-de-destino`, () =>
    api.get<CategoriaDto[]>(`/api/clubes/${clubId}/categorias`),
  );
  const destinos = (categorias.datos ?? []).filter(
    (categoria) => categoria.activa && categoria.categoriaId !== categoriaActualId,
  );
  const elegida = destinos.find((categoria) => categoria.categoriaId === elegidaId);
  const tieneCategoria = categoriaActualId !== undefined;

  return (
    <DialogoConfirmacion
      abierto
      titulo={`${tieneCategoria ? 'Cambiar de categoría a' : 'Ubicar a'} ${nombreCompleto(jugador)}`}
      textoConfirmar={tieneCategoria ? 'Pasar a esa categoría' : 'Ubicar en esa categoría'}
      deshabilitado={!elegida}
      cargando={cargando}
      error={error}
      alConfirmar={() => {
        if (elegida) {
          alConfirmar(elegida);
        }
      }}
      alCancelar={alCancelar}
    >
      <p className="texto-suave">Nació en {jugador.anioNacimiento}.</p>
      {categorias.error && <Aviso tono="error">{categorias.error}</Aviso>}
      {!categorias.datos && categorias.cargando && <p className="texto-suave">Cargando…</p>}
      {categorias.datos && destinos.length === 0 && (
        <p className="texto-suave">El club no tiene otra categoría activa a la que pasarlo.</p>
      )}
      {destinos.length > 0 && (
        <Campo
          etiqueta="Categoría de destino"
          valor={elegidaId}
          alCambiar={setElegidaId}
          opciones={[
            { valor: '', texto: 'Elige una categoría' },
            ...destinos.map((categoria) => ({ valor: categoria.categoriaId, texto: `Categoría ${categoria.anio}` })),
          ]}
        />
      )}
      {elegida && elegida.anio !== jugador.anioNacimiento && (
        <Aviso tono="aviso">
          La categoría {elegida.anio} no es la de su año de nacimiento ({jugador.anioNacimiento}). Quedará
          señalado como fuera de su año.
        </Aviso>
      )}
      {tieneCategoria && <p className="texto-suave">Al cambiar de categoría sale de los equipos en los que está.</p>}
    </DialogoConfirmacion>
  );
}
