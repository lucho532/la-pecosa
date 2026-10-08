import { Navigate } from 'react-router-dom';
import { api } from '../../compartido/api/cliente';
import type { IngresoAprobadoDto, IngresoEnEsperaDto, InvitacionClubDto, Rol } from '../../compartido/api/tipos';
import { useCarga } from '../../compartido/api/useCarga';
import { useClub } from '../contextoClub';
import { SeccionIngresosAprobados } from './SeccionIngresosAprobados';
import { SeccionInvitacionesClub } from './SeccionInvitacionesClub';
import { SeccionSalaDeEspera } from './SeccionSalaDeEspera';

/**
 * Apartado "Ingresos" del club, para su presidente y sus directivos: la sala de espera, las
 * invitaciones enviadas y los ingresos aprobados. La API rechaza a cualquier otro rol; aquí
 * simplemente no se le muestra la pantalla.
 */
export function Ingresos() {
  const { club } = useClub();

  if (club.miRol !== 'PRESIDENTE' && club.miRol !== 'DIRECTIVO') {
    return <Navigate to={`/club/${club.clubId}`} replace />;
  }

  return <SeccionesDeIngresos clubId={club.clubId} miRol={club.miRol} />;
}

/** Carga los datos del apartado y compone sus secciones; cada una recarga las que le afectan. */
function SeccionesDeIngresos({ clubId, miRol }: { clubId: string; miRol: Rol }) {
  const base = `/api/clubes/${clubId}`;
  const enEspera = useCarga(`${clubId}/en-espera`, () =>
    api.get<IngresoEnEsperaDto[]>(`${base}/ingresos/en-espera`),
  );
  const invitaciones = useCarga(`${clubId}/invitaciones`, () =>
    api.get<InvitacionClubDto[]>(`${base}/invitaciones`),
  );
  const aprobados = useCarga(`${clubId}/aprobados`, () =>
    api.get<IngresoAprobadoDto[]>(`${base}/ingresos/aprobados`),
  );

  return (
    <>
      <div className="cabecera">
        <h1>Ingresos</h1>
      </div>
      <SeccionSalaDeEspera
        clubId={clubId}
        miRol={miRol}
        enEspera={enEspera}
        alCambiar={() => {
          aprobados.recargar();
          invitaciones.recargar();
        }}
      />
      <SeccionInvitacionesClub clubId={clubId} invitaciones={invitaciones} />
      <SeccionIngresosAprobados aprobados={aprobados} />
    </>
  );
}
