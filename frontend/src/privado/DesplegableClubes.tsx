import { useId } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSesion } from '../compartido/sesion/useSesion';

/**
 * Desplegable para elegir de cuál de sus clubes quiere ver los datos la persona (constitución
 * §7.3). Solo aparece con más de un club y solo lista los clubes de su sesión; cambiar de club es
 * navegar, y la API vuelve a comprobar la pertenencia. Marca con texto los clubes en los que su
 * ingreso sigue en espera.
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
            {club.estadoIngreso === 'EN_ESPERA' ? ' (en espera)' : ''}
          </option>
        ))}
      </select>
    </div>
  );
}
