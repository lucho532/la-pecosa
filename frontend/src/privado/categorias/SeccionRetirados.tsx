import { api } from '../../compartido/api/cliente';
import type { JugadorRetiradoDto, ReincorporacionDto } from '../../compartido/api/tiposCategorias';
import type { Carga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { Tabla, type Columna } from '../../compartido/componentes/Tabla';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { fecha } from '../../compartido/formato';
import { nombreCompleto } from './textos';
import { useAccion } from './useAccion';

interface Props {
  clubId: string;
  esPresidente: boolean;
  retirados: Carga<JugadorRetiradoDto[]>;
  /** Se llama tras reincorporar a un jugador, para recargar las listas del apartado. */
  alCambiar: () => void;
}

/**
 * Jugadores retirados del club, del retiro más reciente al más antiguo: quién es, su año de
 * nacimiento, quién lo retiró y cuándo (RF-044). El presidente puede reincorporar a cada uno, que
 * vuelve a entrar con su misma cuenta y queda en la categoría activa de su año o sin categoría; el
 * directivo solo ve la lista.
 */
export function SeccionRetirados({ clubId, esPresidente, retirados, alCambiar }: Props) {
  const { ocupado, mensaje, ejecutar } = useAccion();

  const reincorporar = (jugador: JugadorRetiradoDto) =>
    ejecutar(async () => {
      const resultado = await api.post<ReincorporacionDto>(
        `/api/clubes/${clubId}/jugadores/${jugador.usuarioRolId}/reincorporacion`,
      );
      alCambiar();
      const donde =
        resultado.anio === null
          ? 'Quedó sin categoría: el club no tiene activa la de su año de nacimiento.'
          : `Quedó en la categoría ${resultado.anio}.`;
      return `${nombreCompleto(jugador)} vuelve a estar en el club. ${donde} No recupera los equipos que tenía.`;
    });

  const columnas: Columna<JugadorRetiradoDto>[] = [
    { titulo: 'Jugador', celda: (jugador) => <strong>{nombreCompleto(jugador)}</strong> },
    { titulo: 'Año de nacimiento', celda: (jugador) => jugador.anioNacimiento },
    { titulo: 'Lo retiró', celda: (jugador) => jugador.retiradoPor },
    { titulo: 'Retirado el', celda: (jugador) => fecha(jugador.retiradoEn) },
  ];

  if (esPresidente) {
    columnas.push({
      titulo: 'Acciones',
      celda: (jugador) => (
        <Boton variante="secundario" onClick={() => void reincorporar(jugador)} disabled={ocupado}>
          Reincorporar
        </Boton>
      ),
    });
  }

  return (
    <Tarjeta titulo="Retirados">
      <p className="texto-suave">
        Jugadores que se fueron del club. Ya no entran a él, pero sus datos se conservan.
      </p>
      {mensaje && <Aviso tono={mensaje.tono}>{mensaje.texto}</Aviso>}
      {retirados.error && <Aviso tono="error">{retirados.error}</Aviso>}
      {!retirados.datos && retirados.cargando && <p className="texto-suave">Cargando…</p>}
      {retirados.datos && (
        <Tabla
          descripcion="Jugadores retirados del club"
          columnas={columnas}
          filas={retirados.datos}
          clave={(jugador) => jugador.usuarioRolId}
          vacio="El club no ha retirado a ningún jugador."
        />
      )}
    </Tarjeta>
  );
}
