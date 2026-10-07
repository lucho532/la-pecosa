import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { mensajeDe } from '../compartido/api/errores';
import type { ClubDetalleDto, EliminarClubDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Campo } from '../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../compartido/componentes/DialogoConfirmacion';

interface Props {
  club: ClubDetalleDto;
  abierto: boolean;
  alCerrar: () => void;
}

/**
 * Confirmación expresa para eliminar un club dado de baja. Advierte de que no se puede deshacer y
 * solo deja continuar cuando se escribe el nombre del club; la API vuelve a comprobarlo.
 */
export function DialogoEliminarClub({ club, abierto, alCerrar }: Props) {
  const navegar = useNavigate();
  const [nombre, setNombre] = useState('');
  const [error, setError] = useState<string | undefined>();
  const [enviando, setEnviando] = useState(false);

  async function eliminar() {
    setError(undefined);
    setEnviando(true);
    try {
      const datos: EliminarClubDto = { nombreDeConfirmacion: nombre };
      await api.post(`/api/plataforma/clubes/${club.clubId}/eliminacion`, datos);
      navegar('/plataforma', { replace: true });
    } catch (fallo) {
      setError(mensajeDe(fallo));
      setEnviando(false);
    }
  }

  function cerrar() {
    setNombre('');
    setError(undefined);
    alCerrar();
  }

  return (
    <DialogoConfirmacion
      abierto={abierto}
      titulo={`Eliminar ${club.nombre}`}
      textoConfirmar="Eliminar el club para siempre"
      peligro
      deshabilitado={nombre.trim() !== club.nombre}
      cargando={enviando}
      error={error}
      alConfirmar={() => void eliminar()}
      alCancelar={cerrar}
    >
      <Aviso tono="error">
        Esto no se puede deshacer. Se borra toda la información del club: sus integrantes, sus invitaciones y
        su escudo. Las personas que solo pertenecían a este club pierden su cuenta.
      </Aviso>
      <Campo
        etiqueta={`Para confirmar, escribe el nombre del club: ${club.nombre}`}
        valor={nombre}
        alCambiar={setNombre}
        autoComplete="off"
      />
    </DialogoConfirmacion>
  );
}
