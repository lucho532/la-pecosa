import { Tarjeta } from '../compartido/componentes/Tarjeta';
import { nombreDeRol } from '../compartido/formato';
import { useClub } from './contextoClub';

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
          Entraste como <strong>{nombreDeRol(club.miRol)}</strong> de {club.nombre}.
        </p>
        {club.sede && <p className="texto-suave">Sede: {club.sede}</p>}
        {club.direccion && <p className="texto-suave">Dirección: {club.direccion}</p>}
        {contacto && <p className="texto-suave">Contacto: {contacto}</p>}
      </Tarjeta>
    </>
  );
}
