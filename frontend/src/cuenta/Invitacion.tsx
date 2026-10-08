import { Link, useLocation } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import type { InvitacionVigenteDto, TokenDto } from '../compartido/api/tipos';
import { useCarga } from '../compartido/api/useCarga';
import { Aviso } from '../compartido/componentes/Aviso';
import { nombreDeRol } from '../compartido/formato';
import { useSesion } from '../compartido/sesion/useSesion';
import { AceptarInvitacion } from './AceptarInvitacion';
import { DisposicionCuenta } from './DisposicionCuenta';
import { FormularioRegistro } from './FormularioRegistro';

/**
 * Pantalla del enlace de invitación (`/invitacion#<token>`). Muestra el club y el correo, que no
 * se pueden cambiar, y el rol solo si es una invitación de presidente: la que envía el club no
 * nombra ninguno. Ofrece el registro o, si el correo ya tiene cuenta, la aceptación. Si la
 * invitación no sirve, lo explica y no muestra ningún formulario.
 */
export function Invitacion() {
  const token = useLocation().hash.replace(/^#/, '');
  const { estado } = useSesion();
  const { datos: invitacion, error, cargando } = useCarga(token, () => {
    const cuerpo: TokenDto = { token };
    return api.post<InvitacionVigenteDto>('/api/invitaciones/consulta', cuerpo);
  });

  if (error) {
    return (
      <DisposicionCuenta titulo="Invitación">
        <Aviso tono="aviso">{error}</Aviso>
        <Link to="/entrar">Ir a iniciar sesión</Link>
      </DisposicionCuenta>
    );
  }

  if (!invitacion || estado === 'cargando') {
    return (
      <DisposicionCuenta titulo="Invitación">
        <p className="texto-suave">{cargando ? 'Cargando la invitación…' : ''}</p>
      </DisposicionCuenta>
    );
  }

  return (
    <DisposicionCuenta
      titulo={invitacion.nombreClub}
      subtitulo={
        invitacion.pasaPorSalaDeEspera
          ? 'Invitación para registrarte en el club'
          : `Invitación para ser ${nombreDeRol(invitacion.rol).toLowerCase()} del club`
      }
      identidad={invitacion.identidad}
    >
      {invitacion.tieneCuenta ? (
        <AceptarInvitacion token={token} invitacion={invitacion} />
      ) : (
        <FormularioRegistro token={token} invitacion={invitacion} />
      )}
    </DisposicionCuenta>
  );
}
