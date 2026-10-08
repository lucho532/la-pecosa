import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { mensajeDe } from '../../compartido/api/errores';
import type { CategoriaConUbicadosDto, CategoriaDto } from '../../compartido/api/tiposCategorias';
import { Boton } from '../../compartido/componentes/Boton';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { resumenDeUbicados } from './textos';
import type { Mensaje } from './useAccion';

interface Props {
  clubId: string;
  categoria: CategoriaDto;
  /** Se llama al terminar cada acción, salga bien o la rechace la API, con lo que hay que decir. */
  alTerminar: (mensaje: Mensaje) => void;
  /** Se llama, además, cuando la categoría se borró. */
  alBorrar?: () => void;
}

type Pendiente = 'desactivar' | 'borrar' | null;

/**
 * Acciones del presidente sobre una categoría: desactivarla o reactivarla y, si nunca se usó,
 * borrarla. Desactivar y borrar piden confirmación (RF-004, RF-004a). Los botones son una ayuda de
 * pantalla: quien decide es la API.
 */
export function AccionesDeCategoria({ clubId, categoria, alTerminar, alBorrar }: Props) {
  const ruta = `/api/clubes/${clubId}/categorias/${categoria.categoriaId}`;
  const [pendiente, setPendiente] = useState<Pendiente>(null);
  const [ocupado, setOcupado] = useState(false);

  async function ejecutar(accion: () => Promise<string>, despues?: () => void) {
    setOcupado(true);
    try {
      alTerminar({ tono: 'exito', texto: await accion() });
      despues?.();
    } catch (fallo) {
      alTerminar({ tono: 'error', texto: mensajeDe(fallo) });
    } finally {
      setOcupado(false);
      setPendiente(null);
    }
  }

  const desactivar = () =>
    ejecutar(async () => {
      await api.post<CategoriaDto>(`${ruta}/desactivacion`);
      return `Categoría ${categoria.anio} desactivada. Sus entrenadores ya no están asignados a ella.`;
    });

  const reactivar = () =>
    ejecutar(async () => {
      const reactivada = await api.post<CategoriaConUbicadosDto>(`${ruta}/reactivacion`);
      return resumenDeUbicados(reactivada.categoria, reactivada.jugadoresUbicados, 'reactivada');
    });

  const borrar = () =>
    ejecutar(async () => {
      await api.delete(ruta);
      return `Categoría ${categoria.anio} borrada.`;
    }, alBorrar);

  return (
    <span className="fila">
      {categoria.activa ? (
        <Boton variante="secundario" onClick={() => setPendiente('desactivar')} disabled={ocupado}>
          Desactivar
        </Boton>
      ) : (
        <Boton variante="secundario" onClick={() => void reactivar()} cargando={ocupado} textoCargando="Reactivando…">
          Reactivar
        </Boton>
      )}
      {categoria.sePuedeBorrar && (
        <Boton variante="secundario" onClick={() => setPendiente('borrar')} disabled={ocupado}>
          Borrar
        </Boton>
      )}

      <DialogoConfirmacion
        abierto={pendiente === 'desactivar'}
        titulo={`Desactivar la categoría ${categoria.anio}`}
        textoConfirmar="Desactivar la categoría"
        peligro
        cargando={ocupado}
        alConfirmar={() => void desactivar()}
        alCancelar={() => setPendiente(null)}
      >
        <p>
          La categoría quedará inactiva y sus entrenadores dejarán de estar asignados a ella. Si la reactivas
          después, tendrás que volver a asignarlos.
        </p>
        <p className="texto-suave">
          Solo se puede desactivar una categoría sin jugadores: antes pásalos a otra categoría o retíralos del
          club.
        </p>
      </DialogoConfirmacion>

      <DialogoConfirmacion
        abierto={pendiente === 'borrar'}
        titulo={`Borrar la categoría ${categoria.anio}`}
        textoConfirmar="Borrar la categoría"
        peligro
        cargando={ocupado}
        alConfirmar={() => void borrar()}
        alCancelar={() => setPendiente(null)}
      >
        <p>
          La categoría y sus equipos se borrarán del todo. Se puede borrar porque nunca ha tenido jugadores ni
          entrenadores; después podrás volver a crear la categoría de ese año.
        </p>
      </DialogoConfirmacion>
    </span>
  );
}
