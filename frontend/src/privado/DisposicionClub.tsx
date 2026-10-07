import { useEffect } from 'react';
import { NavLink, Outlet, useNavigate, useParams } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { ErrorApi } from '../compartido/api/errores';
import type { ClubDto } from '../compartido/api/tipos';
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
import type { ContextoDelClub } from './contextoClub';
import { DesplegableClubes } from './DesplegableClubes';

/**
 * Armazón de la aplicación del club elegido: menú lateral, cabecera con el escudo, los colores y el
 * nombre del club siempre visibles (constitución §7.2) y cierre de sesión. Al cambiar de club en el
 * desplegable cambia la identidad. Pide el club a la API en cada entrada: es la API
 * la que decide si la persona pertenece a él y si el club está disponible.
 */
export function DisposicionClub() {
  const { clubId = '' } = useParams();
  const { sesion, cerrar } = useSesion();
  const navegar = useNavigate();
  const { datos: club, error, fallo, cargando, fijar } = useCarga(clubId, () => api.get<ClubDto>(`/api/clubes/${clubId}`));
  const codigo = fallo instanceof ErrorApi ? fallo.codigo : undefined;

  useEffect(() => {
    guardarUltimoClub(clubId);
  }, [clubId]);

  // Mientras llega el club se muestra lo que ya trae la sesión, para que su nombre nunca falte.
  const deSesion = sesion?.clubes.find((candidato) => candidato.clubId === clubId);
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
          error && <Aviso tono="error">{error}</Aviso>
        )}
        {!contexto && cargando && <p className="texto-suave">Cargando…</p>}
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
