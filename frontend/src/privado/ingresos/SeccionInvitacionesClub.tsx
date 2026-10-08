import { useState, type FormEvent } from 'react';
import { api } from '../../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { InvitacionClubDto, InvitarAlClubDto } from '../../compartido/api/tipos';
import type { Carga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { Campo } from '../../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { EtiquetaEstadoEnvio, EtiquetaEstadoInvitacion } from '../../compartido/componentes/EtiquetaEstado';
import { Tabla, type Columna } from '../../compartido/componentes/Tabla';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { fecha } from '../../compartido/formato';

interface Props {
  clubId: string;
  invitaciones: Carga<InvitacionClubDto[]>;
}

/**
 * Invitaciones enviadas desde el club: invitar por correo, ver el estado de cada una y reenviar o
 * cancelar las pendientes. La invitación no lleva rol: se decide al aprobar el ingreso.
 */
export function SeccionInvitacionesClub({ clubId, invitaciones }: Props) {
  const base = `/api/clubes/${clubId}/invitaciones`;
  const [correo, setCorreo] = useState('');
  const [errorCorreo, setErrorCorreo] = useState<string | undefined>();
  const [error, setError] = useState<string | null>(null);
  const [hecho, setHecho] = useState<{ tono: 'info' | 'aviso'; texto: string } | null>(null);
  const [ocupado, setOcupado] = useState(false);
  const [porCancelar, setPorCancelar] = useState<InvitacionClubDto | null>(null);

  /** Ejecuta una acción sobre las invitaciones y recarga la lista, también si la API la rechaza. */
  async function ejecutar(accion: () => Promise<InvitacionClubDto>, alTerminar: (invitacion: InvitacionClubDto) => string) {
    setError(null);
    setErrorCorreo(undefined);
    setHecho(null);
    setOcupado(true);
    try {
      const invitacion = await accion();
      setHecho(
        invitacion.estadoEnvio === 'FALLIDO'
          ? { tono: 'aviso', texto: `No se pudo enviar el correo a ${invitacion.correo}; puedes reenviarla.` }
          : { tono: 'info', texto: alTerminar(invitacion) },
      );
      return true;
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.errorDe('correo')) {
        setErrorCorreo(fallo.errorDe('correo'));
      } else {
        setError(mensajeDe(fallo));
      }

      return false;
    } finally {
      setOcupado(false);
      invitaciones.recargar();
    }
  }

  async function invitar(evento: FormEvent) {
    evento.preventDefault();
    const datos: InvitarAlClubDto = { correo };
    const enviada = await ejecutar(
      () => api.post<InvitacionClubDto>(base, datos),
      (invitacion) => `Invitación enviada a ${invitacion.correo}.`,
    );
    if (enviada) {
      setCorreo('');
    }
  }

  const reenviar = (invitacion: InvitacionClubDto) =>
    ejecutar(
      () => api.post<InvitacionClubDto>(`${base}/${invitacion.invitacionId}/reenvio`),
      (nueva) => `Invitación reenviada a ${nueva.correo}. El enlace anterior ya no sirve.`,
    );

  async function cancelar() {
    if (!porCancelar) {
      return;
    }

    const invitacion = porCancelar;
    setPorCancelar(null);
    await ejecutar(
      () => api.post<InvitacionClubDto>(`${base}/${invitacion.invitacionId}/cancelacion`),
      (cancelada) => `Invitación a ${cancelada.correo} cancelada. Su enlace ya no sirve.`,
    );
  }

  const columnas: Columna<InvitacionClubDto>[] = [
    { titulo: 'Correo', celda: (invitacion) => <strong>{invitacion.correo}</strong> },
    {
      titulo: 'Estado',
      celda: (invitacion) => (
        <span className="fila">
          <EtiquetaEstadoInvitacion estado={invitacion.estado} />
          {invitacion.estado === 'PENDIENTE' && invitacion.estadoEnvio === 'FALLIDO' && (
            <EtiquetaEstadoEnvio estado={invitacion.estadoEnvio} />
          )}
        </span>
      ),
    },
    { titulo: 'Enviada por', celda: (invitacion) => invitacion.enviadaPor ?? 'Ya no está en el club' },
    { titulo: 'Enviada', celda: (invitacion) => fecha(invitacion.creadaEn) },
    { titulo: 'Vence', celda: (invitacion) => fecha(invitacion.venceEn) },
    {
      titulo: 'Acciones',
      celda: (invitacion) =>
        invitacion.estado === 'PENDIENTE' && (
          <span className="fila">
            <Boton variante="secundario" onClick={() => void reenviar(invitacion)} disabled={ocupado}>
              Reenviar
            </Boton>
            <Boton variante="secundario" onClick={() => setPorCancelar(invitacion)} disabled={ocupado}>
              Cancelar
            </Boton>
          </span>
        ),
    },
  ];

  return (
    <Tarjeta titulo="Invitaciones">
      <p className="texto-suave">
        Escribe el correo de la persona. Recibirá un enlace para registrarse en el club, que sirve una sola vez
        y vence a los 7 días. Cómo entra (jugador, entrenador o directivo) se decide al aprobar su ingreso.
      </p>
      <form className="fila" onSubmit={(evento) => void invitar(evento)} noValidate>
        <Campo
          etiqueta="Correo de la persona"
          type="email"
          valor={correo}
          alCambiar={setCorreo}
          error={errorCorreo}
          placeholder="correo@ejemplo.com"
          autoComplete="off"
          maxLength={254}
        />
        <Boton type="submit" cargando={ocupado} textoCargando="Enviando…">
          Enviar invitación
        </Boton>
      </form>

      {error && <Aviso tono="error">{error}</Aviso>}
      {hecho && <Aviso tono={hecho.tono}>{hecho.texto}</Aviso>}
      {invitaciones.error && <Aviso tono="error">{invitaciones.error}</Aviso>}
      {!invitaciones.datos && invitaciones.cargando && <p className="texto-suave">Cargando…</p>}

      {invitaciones.datos && (
        <Tabla
          descripcion="Invitaciones enviadas desde el club"
          columnas={columnas}
          filas={invitaciones.datos}
          clave={(invitacion) => invitacion.invitacionId}
          vacio="Todavía no se ha enviado ninguna invitación."
        />
      )}

      <DialogoConfirmacion
        abierto={porCancelar !== null}
        titulo="Cancelar la invitación"
        textoConfirmar="Cancelar la invitación"
        peligro
        alConfirmar={() => void cancelar()}
        alCancelar={() => setPorCancelar(null)}
      >
        <p>
          El enlace enviado a <strong>{porCancelar?.correo}</strong> dejará de servir. Si más adelante quieres
          que entre, envíale una invitación nueva.
        </p>
      </DialogoConfirmacion>
    </Tarjeta>
  );
}
