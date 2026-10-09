import { useEffect } from 'react';
import { NavLink, Outlet, useNavigate, useParams } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { ErrorApi } from '../compartido/api/errores';
import type { ClubDeSesionDto, ClubDto } from '../compartido/api/tipos';
import { useCarga } from '../compartido/api/useCarga';
import { Aviso } from '../compartido/componentes/Aviso';
import { Avatar, inicialesDe } from '../compartido/componentes/Avatar';
import { Boton } from '../compartido/componentes/Boton';
import { BotonTema } from '../compartido/componentes/BotonTema';
import { nombreDeRol } from '../compartido/formato';
import { guardarUltimoClub } from '../compartido/sesion/ultimoClub';
import { useSesion } from '../compartido/sesion/useSesion';
import { Escudo, IdentidadClub } from '../compartido/tema/IdentidadClub';
import { AvisoClubNoDisponible, esClubNoDisponible } from './AvisoClubNoDisponible';
import { AvisoRetirado } from './AvisoRetirado';
import { puedeVerCategorias } from './categorias/textos';
import type { ContextoDelClub } from './contextoClub';
import { DesplegableClubes } from './DesplegableClubes';
import { SalaDeEspera } from './SalaDeEspera';

/** Códigos con los que la API dice que la sesión guardada ya no refleja la relación con el club. */
const SESION_DESACTUALIZADA = ['ingreso_en_espera', 'integrante_retirado', 'no_encontrado'];

/**
 * Entrada al club elegido. Antes de pedir nada al club mira el estado de ingreso que trae la
 * sesión: quien está en espera ve solo la sala de espera y no se llama a la API del club, que se
 * lo negaría. Lo mismo quien fue retirado del club: solo ve el aviso de que ya no está en él. Los
 * demás, también quien acaba de registrarse con una invitación, ven la aplicación del club.
 */
export function DisposicionClub() {
  const { clubId = '' } = useParams();
  const { sesion } = useSesion();
  const deSesion = sesion?.clubes.find((candidato) => candidato.clubId === clubId);

  useEffect(() => {
    guardarUltimoClub(clubId);
  }, [clubId]);

  if (deSesion?.estadoIngreso === 'EN_ESPERA') {
    return <SalaDeEspera club={deSesion} />;
  }

  // Igual con quien fue retirado del club: solo ve el aviso, sin pedir nada al club (RF-043).
  if (deSesion?.retirado) {
    return <AvisoRetirado club={deSesion} />;
  }

  return <AplicacionDelClub clubId={clubId} deSesion={deSesion} />;
}

interface Props {
  clubId: string;
  /** El club tal como lo trae la sesión, para que su nombre nunca falte mientras llega de la API. */
  deSesion: ClubDeSesionDto | undefined;
}

/**
 * Armazón de la aplicación del club elegido: menú lateral, cabecera con el escudo, los colores y el
 * nombre del club siempre visibles (constitución §7.2) y cierre de sesión. Al cambiar de club en el
 * desplegable cambia la identidad. Pide el club a la API en cada entrada: es la API
 * la que decide si la persona pertenece a él y si el club está disponible.
 */
function AplicacionDelClub({ clubId, deSesion }: Props) {
  const { sesion, cerrar, recargar } = useSesion();
  const navegar = useNavigate();
  const { datos: club, error, fallo, cargando, fijar } = useCarga(clubId, () => api.get<ClubDto>(`/api/clubes/${clubId}`));
  const codigo = fallo instanceof ErrorApi ? fallo.codigo : undefined;
  const sesionDesactualizada = codigo !== undefined && SESION_DESACTUALIZADA.includes(codigo);

  // La API manda: si dice que la persona está en espera o que el club ya no es suyo (la
  // rechazaron con la pantalla abierta), se recarga la sesión. Con ella al día, quien sigue en
  // espera ve la sala de espera y quien ya no está en el club pasa a uno de los suyos, o a
  // iniciar sesión si su cuenta ya no existe.
  useEffect(() => {
    if (sesionDesactualizada) {
      void recargar();
    }
  }, [sesionDesactualizada, recargar]);

  const nombre = club?.nombre ?? deSesion?.nombre ?? '';
  const rol = club?.miRol ?? deSesion?.rol;
  const identidad = club?.identidad ?? deSesion?.identidad;

  function cerrarSesion() {
    cerrar();
    navegar('/entrar', { replace: true });
  }

  const contexto: ContextoDelClub | null = club ? { club, fijarClub: fijar } : null;

  return (
    <IdentidadClub identidad={identidad} className="disposicion">
      <aside className="lateral">
        <div className="lateral-marca">
          <Escudo identidad={identidad} nombre={nombre} />
          <span>{nombre}</span>
        </div>
        <DesplegableClubes clubId={clubId} />
        <nav className="lateral-menu" aria-label={`Menú de ${nombre}`}>
          <NavLink to={`/club/${clubId}`} end>
            Inicio
          </NavLink>
          {rol === 'PRESIDENTE' && <NavLink to={`/club/${clubId}/ingresos`}>Ingresos</NavLink>}
          {rol && puedeVerCategorias(rol) && <NavLink to={`/club/${clubId}/categorias`}>Categorías</NavLink>}
          {rol === 'PRESIDENTE' && <NavLink to={`/club/${clubId}/configuracion`}>Datos del club</NavLink>}
        </nav>
        {deSesion && rol && (
          <div className="lateral-pie">
            <span>
              {deSesion.nombres} {deSesion.apellidos}
            </span>
            <span>{nombreDeRol(rol)}</span>
          </div>
        )}
        <Avatar iniciales={deSesion ? inicialesDe(deSesion.nombres, deSesion.apellidos) : ''} />
        <BotonTema enLateral />
        <Boton variante="lateral" onClick={cerrarSesion}>
          Cerrar sesión
        </Boton>
      </aside>
      <main className="principal">
        {esClubNoDisponible(codigo) ? (
          <AvisoClubNoDisponible codigo={codigo} tieneOtrosClubes={(sesion?.clubes.length ?? 0) > 1} />
        ) : (
          error && !sesionDesactualizada && <Aviso tono="error">{error}</Aviso>
        )}
        {!contexto && (cargando || sesionDesactualizada) && <p className="texto-suave">Cargando…</p>}
        {club?.estado === 'SUSPENDIDO' && (
          <Aviso tono="aviso">
            <strong>Club suspendido.</strong> Solo tú, como presidente, puedes entrar; los demás integrantes ven
            un aviso de incidencia temporal.
          </Aviso>
        )}
        {contexto && <Outlet context={contexto} />}
      </main>
    </IdentidadClub>
  );
}
