import { useId } from 'react';
import { useNavigate } from 'react-router-dom';
import type { ClubDeSesionDto } from '../compartido/api/tipos';
import { useSesion } from '../compartido/sesion/useSesion';

/** Lo que hay que decir de un club en el que la persona todavía no entra o ya no entra. */
function marcaDe(club: ClubDeSesionDto): string {
  if (club.retirado) {
    return ' (retirado)';
  }

  return club.estadoIngreso === 'EN_ESPERA' ? ' (en espera)' : '';
}

/**
 * Desplegable para elegir de cuál de sus clubes quiere ver los datos la persona (constitución
 * §7.3). Solo aparece con más de un club y solo lista los clubes de su sesión; cambiar de club es
 * navegar, y la API vuelve a comprobar la pertenencia. Marca con texto los clubes en los que su
 * ingreso sigue en espera y aquellos de los que fue retirada.
 */
export function DesplegableClubes({ clubId }: { clubId: string }) {
  const { sesion } = useSesion();
  const navegar = useNavigate();
  const id = useId();
  const clubes = sesion?.clubes ?? [];

  if (clubes.length < 2) {
    return null;
  }

  return (
    <div className="campo">
      <label htmlFor={id}>Club</label>
      <select id={id} value={clubId} onChange={(evento) => navegar(`/club/${evento.target.value}`)}>
        {clubes.map((club) => (
          <option key={club.clubId} value={club.clubId}>
            {club.nombre}
            {marcaDe(club)}
          </option>
        ))}
      </select>
    </div>
  );
}
