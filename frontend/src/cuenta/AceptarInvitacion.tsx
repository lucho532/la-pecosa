import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { mensajeDe } from '../compartido/api/errores';
import type { ClubDeSesionDto, InvitacionVigenteDto, TokenDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Boton } from '../compartido/componentes/Boton';
import { nombreDeRol } from '../compartido/formato';
import { useSesion } from '../compartido/sesion/useSesion';

interface Props {
  token: string;
  invitacion: InvitacionVigenteDto;
}

/**
 * Invitación a un correo que ya tiene cuenta: no se registra de nuevo. Pide iniciar sesión con
 * esa cuenta y después ofrece aceptar; si la sesión abierta es de otro correo, lo explica. Nombra
 * siempre el club y el rol y no pide ningún dato: al aceptar, la persona entra directamente al
 * club. Si ya está en ese club, muestra el mensaje de la API y no cambia nada.
 */
export function AceptarInvitacion({ token, invitacion }: Props) {
  const { sesion, recargar, cerrar } = useSesion();
  const navegar = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function aceptar() {
    setError(null);
    setEnviando(true);
    try {
      const cuerpo: TokenDto = { token };
      const club = await api.post<ClubDeSesionDto>('/api/invitaciones/aceptacion', cuerpo);
      await recargar();
      navegar(`/club/${club.clubId}`, { replace: true });
    } catch (fallo) {
      setError(mensajeDe(fallo));
      setEnviando(false);
    }
  }

  if (!sesion) {
    return (
      <>
        <Aviso tono="info">
          Ya tienes una cuenta con el correo {invitacion.correo}. Inicia sesión con ella para aceptar la
          invitación; no hace falta registrarse de nuevo.
        </Aviso>
        {/* El token viaja en el estado de la navegación para volver aquí después de entrar. */}
        <Link className="boton boton-principal" to="/entrar" state={{ desde: `/invitacion#${token}` }}>
          Iniciar sesión
        </Link>
      </>
    );
  }

  if (sesion.correo.toLowerCase() !== invitacion.correo.toLowerCase()) {
    return (
      <>
        <Aviso tono="aviso">
          Esta invitación se envió a {invitacion.correo}, pero tienes abierta la sesión de {sesion.correo}.
          Cierra sesión y entra con la cuenta de ese correo.
        </Aviso>
        <Boton variante="secundario" onClick={cerrar}>
          Cerrar sesión
        </Boton>
      </>
    );
  }

  return (
    <>
      <p>
        Te invitaron a <strong>{invitacion.nombreClub}</strong> como{' '}
        <strong>{nombreDeRol(invitacion.rol).toLowerCase()}</strong>. Al aceptar, ese club se suma a los que
        ya tienes, con la misma cuenta, y entras a él con ese rol.
      </p>
      {error && <Aviso tono="error">{error}</Aviso>}
      <Boton onClick={() => void aceptar()} cargando={enviando} textoCargando="Aceptando…">
        Aceptar
      </Boton>
    </>
  );
}
