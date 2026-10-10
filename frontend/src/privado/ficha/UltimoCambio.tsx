import type { UltimoCambioDto } from '../../compartido/api/tiposFicha';
import { fechaYHora } from '../../compartido/formato';

/**
 * Línea con el último cambio de la ficha: cuándo fue y quién lo hizo. Cuando lo hizo la familia
 * dice "la cuenta del jugador", en lugar de repetir el nombre del jugador. No se pinta si nadie ha
 * cambiado la ficha. No es un historial: solo existe el último cambio.
 */
export function UltimoCambio({ cambio }: { cambio: UltimoCambioDto | undefined }) {
  if (!cambio) {
    return null;
  }

  return (
    <p className="texto-suave">
      Último cambio: {fechaYHora(cambio.fecha)}, por {cambio.porLaCuentaDelJugador ? 'la cuenta del jugador' : cambio.autor}.
    </p>
  );
}
