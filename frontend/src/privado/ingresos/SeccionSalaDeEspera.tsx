import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { AprobarIngresoDto, IngresoAprobadoDto, IngresoEnEsperaDto, Rol, RolDeIngreso } from '../../compartido/api/tipos';
import type { Carga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { dia, fechaYHora, nombreDeRol, nombreDeTipoDocumento } from '../../compartido/formato';
import { DialogoAprobarIngreso } from './DialogoAprobarIngreso';

interface Props {
  clubId: string;
  /** Rol en el club de quien mira la sala de espera: decide qué roles puede asignar. */
  miRol: Rol;
  enEspera: Carga<IngresoEnEsperaDto[]>;
  /** Se llama tras aprobar o rechazar (o intentarlo), para recargar las otras listas del apartado. */
  alCambiar: () => void;
}

/** La API responde 409 o 404 cuando otra persona se adelantó: el ingreso ya no está pendiente. */
const yaNoEstaPendiente = (fallo: unknown): fallo is ErrorApi =>
  fallo instanceof ErrorApi && (fallo.status === 409 || fallo.status === 404);

/**
 * Sala de espera del club: las personas registradas pendientes de aprobación, de la más antigua a
 * la más reciente, cada una como una ficha con los datos con los que se registró. Desde aquí se
 * aprueba (eligiendo cómo entra) o se rechaza un ingreso.
 */
export function SeccionSalaDeEspera({ clubId, miRol, enEspera, alCambiar }: Props) {
  const base = `/api/clubes/${clubId}/ingresos`;
  const [porAprobar, setPorAprobar] = useState<IngresoEnEsperaDto | null>(null);
  const [porRechazar, setPorRechazar] = useState<IngresoEnEsperaDto | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [errorDialogo, setErrorDialogo] = useState<string | undefined>();
  const [resultado, setResultado] = useState<{ tono: 'exito' | 'aviso'; texto: string } | null>(null);

  function abrir(fijar: (persona: IngresoEnEsperaDto) => void, persona: IngresoEnEsperaDto) {
    setErrorDialogo(undefined);
    setResultado(null);
    fijar(persona);
  }

  /** Ejecuta la acción del diálogo abierto, lo cierra si terminó y recarga lo que cambió. */
  async function ejecutar(accion: () => Promise<string>) {
    setEnviando(true);
    setErrorDialogo(undefined);
    try {
      setResultado({ tono: 'exito', texto: await accion() });
      setPorAprobar(null);
      setPorRechazar(null);
    } catch (fallo) {
      if (yaNoEstaPendiente(fallo)) {
        setResultado({ tono: 'aviso', texto: fallo.title });
        setPorAprobar(null);
        setPorRechazar(null);
      } else {
        setErrorDialogo(mensajeDe(fallo));
      }
    } finally {
      setEnviando(false);
      enEspera.recargar();
      alCambiar();
    }
  }

  const aprobar = (persona: IngresoEnEsperaDto, rol: RolDeIngreso) =>
    ejecutar(async () => {
      const datos: AprobarIngresoDto = { rol };
      const aprobado = await api.post<IngresoAprobadoDto>(`${base}/${persona.usuarioRolId}/aprobacion`, datos);
      return `${aprobado.nombres} ${aprobado.apellidos} ya entra al club como ${nombreDeRol(aprobado.rolDeIngreso).toLowerCase()}.`;
    });

  const rechazar = (persona: IngresoEnEsperaDto) =>
    ejecutar(async () => {
      await api.post(`${base}/${persona.usuarioRolId}/rechazo`);
      return `Se rechazó el ingreso de ${persona.nombres} ${persona.apellidos} y se borró su registro.`;
    });

  return (
    <Tarjeta titulo="Sala de espera">
      {resultado && <Aviso tono={resultado.tono}>{resultado.texto}</Aviso>}
      {enEspera.error && <Aviso tono="error">{enEspera.error}</Aviso>}
      {!enEspera.datos && enEspera.cargando && <p className="texto-suave">Cargando…</p>}
      {enEspera.datos?.length === 0 && <p className="texto-suave">No hay nadie esperando aprobación.</p>}

      {enEspera.datos?.map((persona) => (
        <article key={persona.usuarioRolId} className="ficha">
          <h3>
            {persona.nombres} {persona.apellidos}
          </h3>
          <dl className="ficha-datos">
            <Dato nombre={nombreDeTipoDocumento(persona.tipoDocumento)} valor={persona.numeroDocumento} />
            <Dato nombre="Fecha de nacimiento" valor={dia(persona.fechaNacimiento)} />
            <Dato nombre="Correo" valor={persona.correo} />
            <Dato nombre="Celular" valor={persona.celular} />
            <Dato nombre="Padre, madre o responsable" valor={persona.nombreResponsable ?? 'No indicado'} />
            <Dato nombre="Se registró" valor={fechaYHora(persona.registradoEn)} />
          </dl>
          <div className="fila">
            <Boton onClick={() => abrir(setPorAprobar, persona)}>Aprobar</Boton>
            <Boton variante="secundario" onClick={() => abrir(setPorRechazar, persona)}>
              Rechazar
            </Boton>
          </div>
        </article>
      ))}

      {porAprobar && (
        <DialogoAprobarIngreso
          key={porAprobar.usuarioRolId}
          persona={porAprobar}
          miRol={miRol}
          cargando={enviando}
          error={errorDialogo}
          alConfirmar={(rol) => void aprobar(porAprobar, rol)}
          alCancelar={() => setPorAprobar(null)}
        />
      )}

      <DialogoConfirmacion
        abierto={porRechazar !== null}
        titulo={`Rechazar el ingreso de ${porRechazar?.nombres ?? ''} ${porRechazar?.apellidos ?? ''}`}
        textoConfirmar="Rechazar y borrar el registro"
        peligro
        cargando={enviando}
        error={errorDialogo}
        alConfirmar={() => {
          if (porRechazar) {
            void rechazar(porRechazar);
          }
        }}
        alCancelar={() => setPorRechazar(null)}
      >
        <p>
          Su registro se borrará del club y no quedará ningún dato suyo en él. Esta persona solo podrá volver si
          le envías una invitación nueva y se registra otra vez.
        </p>
        <p className="texto-suave">No se le envía ningún aviso.</p>
      </DialogoConfirmacion>
    </Tarjeta>
  );
}

function Dato({ nombre, valor }: { nombre: string; valor: string }) {
  return (
    <div>
      <dt>{nombre}</dt>
      <dd>{valor}</dd>
    </div>
  );
}
