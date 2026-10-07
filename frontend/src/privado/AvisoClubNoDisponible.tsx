import { Aviso } from '../compartido/componentes/Aviso';

/** Códigos con los que la API dice que el club existe pero no está disponible para la persona. */
export type CodigoNoDisponible = 'club_suspendido' | 'club_dado_de_baja';

/** Indica si un código de error de la API es de club no disponible. */
export function esClubNoDisponible(codigo: string | undefined): codigo is CodigoNoDisponible {
  return codigo === 'club_suspendido' || codigo === 'club_dado_de_baja';
}

interface Props {
  codigo: CodigoNoDisponible;
  /** Verdadero si la persona pertenece a más clubes y puede pasar a otro con el desplegable. */
  tieneOtrosClubes: boolean;
}

/**
 * Lo que ve un integrante cuando su club está suspendido (y no es su presidente) o dado de baja.
 * No muestra ningún dato del club. Aparece también si el estado cambia con la sesión ya abierta,
 * porque la API lo comprueba en cada petición (RF-030).
 */
export function AvisoClubNoDisponible({ codigo, tieneOtrosClubes }: Props) {
  return (
    <>
      <h1>{codigo === 'club_suspendido' ? 'Incidencia temporal' : 'Club no disponible'}</h1>
      <Aviso tono="aviso">
        {codigo === 'club_suspendido'
          ? 'Hay una incidencia temporal con este club. Comunícate con el presidente del club.'
          : 'Este club no está disponible.'}
      </Aviso>
      {tieneOtrosClubes && <p className="texto-suave">Puedes pasar a otro de tus clubes con el desplegable.</p>}
    </>
  );
}
