import { Link } from 'react-router-dom';
import { useFotoPerfil } from '../sesion/useFotoPerfil';

interface Props {
  /** Lo que se muestra cuando la cuenta no tiene foto. */
  iniciales: string;
}

/** Iniciales de una persona a partir de sus nombres y apellidos. */
export function inicialesDe(nombres: string, apellidos: string): string {
  return `${nombres.trim().charAt(0)}${apellidos.trim().charAt(0)}`.toUpperCase();
}

/**
 * Foto de perfil de la cuenta con sesión en la cabecera o, si no tiene, sus iniciales. Enlaza a
 * "Mi perfil", donde se carga, se cambia y se quita.
 */
export function Avatar({ iniciales }: Props) {
  const foto = useFotoPerfil();

  return (
    <Link to="/perfil" className="fila" style={{ color: 'inherit', textDecoration: 'none' }}>
      {foto ? (
        <img className="distintivo" src={foto} alt="Tu foto de perfil" />
      ) : (
        <span className="distintivo" aria-hidden="true">
          {iniciales}
        </span>
      )}
      <span>Mi perfil</span>
    </Link>
  );
}
