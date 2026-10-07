import { Navigate } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import type { ActualizarConfiguracionClubDto, ClubDto } from '../compartido/api/tipos';
import { FormularioDatosClub } from '../compartido/componentes/FormularioDatosClub';
import { Tarjeta } from '../compartido/componentes/Tarjeta';
import { useSesion } from '../compartido/sesion/useSesion';
import { useClub } from './contextoClub';

/**
 * Datos del club, para su presidente: nombre, sede, dirección y contacto. No ofrece cambiar el
 * escudo ni los colores, que solo configura la plataforma. La API rechaza a cualquier otro rol;
 * aquí simplemente no se le muestra la pantalla.
 */
export function ConfiguracionClub() {
  const { club, fijarClub } = useClub();
  const { recargar } = useSesion();

  if (club.miRol !== 'PRESIDENTE') {
    return <Navigate to={`/club/${club.clubId}`} replace />;
  }

  async function guardar(datos: ActualizarConfiguracionClubDto) {
    fijarClub(await api.put<ClubDto>(`/api/clubes/${club.clubId}/configuracion`, datos));
    // El nombre del club también está en la sesión (desplegable y cabecera).
    await recargar();
  }

  return (
    <>
      <div className="cabecera">
        <h1>Datos del club</h1>
      </div>
      <Tarjeta>
        <FormularioDatosClub key={club.clubId} inicial={club} alGuardar={guardar} />
        <p className="texto-suave">El escudo y los colores del club los configura la plataforma.</p>
      </Tarjeta>
    </>
  );
}
