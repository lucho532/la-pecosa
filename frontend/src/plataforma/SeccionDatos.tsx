import { api } from '../compartido/api/cliente';
import type { ActualizarConfiguracionClubDto, ClubDetalleDto } from '../compartido/api/tipos';
import { FormularioDatosClub } from '../compartido/componentes/FormularioDatosClub';
import { Tarjeta } from '../compartido/componentes/Tarjeta';
import type { PropsSeccion } from './DetalleClub';

/** Datos de un club (nombre, sede, dirección y contacto), editables por el desarrollador. */
export function SeccionDatos({ club, alActualizar }: PropsSeccion) {
  async function guardar(datos: ActualizarConfiguracionClubDto) {
    alActualizar(await api.put<ClubDetalleDto>(`/api/plataforma/clubes/${club.clubId}/configuracion`, datos));
  }

  return (
    <Tarjeta titulo="Datos del club">
      <FormularioDatosClub key={club.clubId} inicial={club} alGuardar={guardar} />
    </Tarjeta>
  );
}
