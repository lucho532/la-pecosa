import { useNavigate } from 'react-router-dom';
import type { ClubDeSesionDto } from '../compartido/api/tiposSesion';
import { Boton } from '../compartido/componentes/Boton';
import { olvidarJugadorElegido } from '../compartido/sesion/jugadorElegido';

interface Props {
  club: ClubDeSesionDto;
  variante?: 'secundario' | 'lateral';
}

/**
 * "Cambiar de jugador": olvida con cuál continúa la familia en ese club y la devuelve a la lista
 * de sus jugadores, sin cerrar sesión (RF-025). Solo se pinta cuando hay entre quién elegir: no
 * aparece con un solo jugador ni en una sesión iniciada con el documento de uno (RF-026).
 */
export function BotonCambiarJugador({ club, variante = 'secundario' }: Props) {
  const navegar = useNavigate();

  if (club.jugadores.length === 0) {
    return null;
  }

  function cambiar() {
    olvidarJugadorElegido(club.clubId);
    navegar(`/club/${club.clubId}`, { replace: true });
  }

  return (
    <Boton variante={variante} onClick={cambiar}>
      Cambiar de jugador
    </Boton>
  );
}
