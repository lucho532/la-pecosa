import { Link } from 'react-router-dom';
import { Tarjeta } from '../compartido/componentes/Tarjeta';
import { nombreDeRol } from '../compartido/formato';
import { useClub } from './contextoClub';
import { TarjetaMiCategoria } from './TarjetaMiCategoria';

/** Pantalla de inicio de la aplicación del club: su nombre y lo que ese club ha publicado de sí. */
export function InicioClub() {
  const { club } = useClub();
  const contacto = [club.correoContacto, club.telefonoContacto].filter(Boolean).join(' · ');

  return (
    <>
      <div className="cabecera">
        <h1>{club.nombre}</h1>
      </div>
      <Tarjeta titulo="Tu club">
        <p>
          Tu rol en este club: <strong>{nombreDeRol(club.miRol)}</strong>
        </p>
        {club.sede && <p className="texto-suave">Sede: {club.sede}</p>}
        {club.direccion && <p className="texto-suave">Dirección: {club.direccion}</p>}
        {contacto && <p className="texto-suave">Contacto: {contacto}</p>}
        {club.miRol === 'PRESIDENTE' && <Link to={`/club/${club.clubId}/configuracion`}>Editar los datos del club</Link>}
      </Tarjeta>
      {club.miRol === 'JUGADOR' && <TarjetaMiCategoria clubId={club.clubId} />}
    </>
  );
}
