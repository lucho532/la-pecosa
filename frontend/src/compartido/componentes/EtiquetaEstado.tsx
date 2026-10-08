import type { EstadoClub, EstadoEnvio, EstadoInvitacion } from '../api/tipos';
import type { Tono } from './Aviso';

interface Props {
  tono: Tono;
  texto: string;
}

/** Etiqueta de estado. El estado se dice siempre con texto, nunca solo con color (RF-034). */
export function EtiquetaEstado({ tono, texto }: Props) {
  return <span className={`etiqueta tono-${tono}`}>{texto}</span>;
}

const ESTADOS_CLUB: Record<EstadoClub, Props> = {
  ACTIVO: { tono: 'exito', texto: 'Activo' },
  SUSPENDIDO: { tono: 'aviso', texto: 'Suspendido' },
  DADO_DE_BAJA: { tono: 'error', texto: 'Dado de baja' },
};

/** Etiqueta con el estado de un club. */
export function EtiquetaEstadoClub({ estado }: { estado: EstadoClub }) {
  return <EtiquetaEstado {...ESTADOS_CLUB[estado]} />;
}

const ESTADOS_ENVIO: Record<EstadoEnvio, Props> = {
  PENDIENTE: { tono: 'neutro', texto: 'Envío pendiente' },
  ENVIADO: { tono: 'exito', texto: 'Correo enviado' },
  FALLIDO: { tono: 'error', texto: 'El correo falló' },
};

/** Etiqueta con el resultado del envío de una invitación. */
export function EtiquetaEstadoEnvio({ estado }: { estado: EstadoEnvio }) {
  return <EtiquetaEstado {...ESTADOS_ENVIO[estado]} />;
}

const ESTADOS_INVITACION: Record<EstadoInvitacion, Props> = {
  PENDIENTE: { tono: 'info', texto: 'Pendiente' },
  USADA: { tono: 'exito', texto: 'Usada' },
  VENCIDA: { tono: 'aviso', texto: 'Vencida' },
  CANCELADA: { tono: 'neutro', texto: 'Cancelada' },
};

/** Etiqueta con el estado de una invitación del club. */
export function EtiquetaEstadoInvitacion({ estado }: { estado: EstadoInvitacion }) {
  return <EtiquetaEstado {...ESTADOS_INVITACION[estado]} />;
}
