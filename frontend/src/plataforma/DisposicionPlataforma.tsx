import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { Boton } from '../compartido/componentes/Boton';
import { useSesion } from '../compartido/sesion/useSesion';

/**
 * Armazón del panel de administración de la plataforma: menú lateral, identidad neutra de la
 * plataforma (nunca la de un club) y cierre de sesión.
 */
export function DisposicionPlataforma() {
  const { sesion, cerrar } = useSesion();
  const navegar = useNavigate();

  function cerrarSesion() {
    cerrar();
    navegar('/entrar', { replace: true });
  }

  return (
    <div className="disposicion">
      <aside className="lateral">
        <div className="lateral-marca">
          <span>
            La Pecosa
            <br />
            <small>Administración</small>
          </span>
        </div>
        <nav className="lateral-menu" aria-label="Panel de administración">
          <NavLink to="/plataforma" end>
            Clubes
          </NavLink>
        </nav>
        <div className="lateral-pie">
          <span>{sesion?.correo}</span>
          <span>Desarrollador</span>
        </div>
        <Boton variante="lateral" onClick={cerrarSesion}>
          Cerrar sesión
        </Boton>
      </aside>
      <main className="principal">
        <Outlet />
      </main>
    </div>
  );
}
