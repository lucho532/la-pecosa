import { Navigate } from 'react-router-dom';
import { Aviso } from './compartido/componentes/Aviso';
import { Boton } from './compartido/componentes/Boton';
import { clubDeEntrada } from './compartido/sesion/ultimoClub';
import { useSesion } from './compartido/sesion/useSesion';

/**
 * Raíz de la aplicación: lleva a cada quien a su zona. Sin sesión, al inicio de sesión; el
 * DESARROLLADOR, al panel; el resto, a su único club o al último que eligió.
 */
export function Inicio() {
  const { estado, sesion, cerrar } = useSesion();

  if (estado === 'cargando') {
    return <p className="centrado texto-suave">Cargando…</p>;
  }

  if (!sesion) {
    return <Navigate to="/entrar" replace />;
  }

  if (sesion.esDesarrollador) {
    return <Navigate to="/plataforma" replace />;
  }

  const clubId = clubDeEntrada(sesion.clubes);
  if (clubId) {
    return <Navigate to={`/club/${clubId}`} replace />;
  }

  return (
    <div className="centrado">
      <Aviso tono="aviso">Tu cuenta no pertenece a ningún club.</Aviso>
      <Boton variante="secundario" onClick={cerrar}>
        Cerrar sesión
      </Boton>
    </div>
  );
}
