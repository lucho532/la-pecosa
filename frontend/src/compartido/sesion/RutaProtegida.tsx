import { Navigate, Outlet, useLocation, useParams } from 'react-router-dom';
import { clubDeEntrada } from './ultimoClub';
import { useSesion } from './useSesion';

interface Props {
  /**
   * `desarrollador`: solo la cuenta DESARROLLADOR. `integrante`: solo quien pertenece al club de
   * la ruta. `cualquiera`: basta con tener sesión.
   */
  quien: 'desarrollador' | 'integrante' | 'cualquiera';
}

/**
 * Exige sesión para ver una ruta y lleva a cada quien a su zona. Es una ayuda de navegación: la
 * autorización real la hace la API en cada petición (constitución §15).
 */
export function RutaProtegida({ quien }: Props) {
  const { estado, sesion } = useSesion();
  const ubicacion = useLocation();
  const { clubId } = useParams();

  if (estado === 'cargando') {
    return <p className="centrado texto-suave">Cargando…</p>;
  }

  if (!sesion) {
    return <Navigate to="/entrar" replace state={{ desde: ubicacion.pathname }} />;
  }

  if (quien === 'desarrollador' && !sesion.esDesarrollador) {
    return <Navigate to="/" replace />;
  }

  if (quien === 'integrante') {
    if (sesion.esDesarrollador) {
      return <Navigate to="/plataforma" replace />;
    }

    // Quien intenta abrir un club que no es suyo vuelve a uno propio.
    if (!sesion.clubes.some((club) => club.clubId === clubId)) {
      const propio = clubDeEntrada(sesion.clubes);
      return <Navigate to={propio ? `/club/${propio}` : '/'} replace />;
    }
  }

  return <Outlet />;
}
