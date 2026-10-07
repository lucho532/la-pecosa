import { useState } from 'react';
import { api } from '../compartido/api/cliente';
import { mensajeDe } from '../compartido/api/errores';
import type { ClubDetalleDto, PresidenteDto, RetirarPresidenteDto } from '../compartido/api/tipos';
import { Boton } from '../compartido/componentes/Boton';
import { Campo } from '../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../compartido/componentes/DialogoConfirmacion';
import { Tarjeta } from '../compartido/componentes/Tarjeta';
import type { PropsSeccion } from './DetalleClub';

type Accion = RetirarPresidenteDto['accion'];
type RolNuevo = NonNullable<RetirarPresidenteDto['rolNuevo']>;

/**
 * Presidentes registrados de un club. Para quitarle el rol a uno hay que elegir entre asignarle
 * otro rol o eliminarlo del club; la API lo impide si es el único presidente.
 */
export function SeccionPresidentes({ club, alActualizar }: PropsSeccion) {
  const [elegido, setElegido] = useState<PresidenteDto | null>(null);
  const [accion, setAccion] = useState<Accion | null>(null);
  const [rolNuevo, setRolNuevo] = useState<RolNuevo>('DIRECTIVO');
  const [error, setError] = useState<string | undefined>();
  const [enviando, setEnviando] = useState(false);

  function abrir(presidente: PresidenteDto) {
    setElegido(presidente);
    setAccion(null);
    setError(undefined);
  }

  async function confirmar() {
    if (!elegido || !accion) {
      return;
    }

    setEnviando(true);
    setError(undefined);
    try {
      const datos: RetirarPresidenteDto = accion === 'ASIGNAR_ROL' ? { accion, rolNuevo } : { accion };
      const ruta = `/api/plataforma/clubes/${club.clubId}/presidentes/${elegido.usuarioRolId}/retiro`;
      alActualizar(await api.post<ClubDetalleDto>(ruta, datos));
      setElegido(null);
    } catch (fallo) {
      setError(mensajeDe(fallo));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <Tarjeta titulo="Presidentes">
      {club.presidentes.length === 0 && (
        <p className="texto-suave">Ningún presidente se ha registrado todavía.</p>
      )}

      {club.presidentes.map((presidente) => (
        <div key={presidente.usuarioRolId} className="fila">
          <div style={{ flex: '1 1 200px', minWidth: 0 }}>
            <strong>
              {presidente.nombres} {presidente.apellidos}
            </strong>
            <div className="texto-suave">{presidente.correo}</div>
          </div>
          <Boton variante="secundario" onClick={() => abrir(presidente)}>
            Quitar rol
          </Boton>
        </div>
      ))}

      <DialogoConfirmacion
        abierto={elegido !== null}
        titulo={`Quitar el rol de presidente a ${elegido?.nombres ?? ''} ${elegido?.apellidos ?? ''}`}
        textoConfirmar={accion === 'ELIMINAR_DEL_CLUB' ? 'Eliminar del club' : 'Quitar rol'}
        peligro={accion === 'ELIMINAR_DEL_CLUB'}
        deshabilitado={accion === null}
        cargando={enviando}
        error={error}
        alConfirmar={() => void confirmar()}
        alCancelar={() => setElegido(null)}
      >
        <p>Elige qué pasa con esta persona en el club:</p>
        <label className="fila">
          <input
            type="radio"
            name="accion"
            checked={accion === 'ASIGNAR_ROL'}
            onChange={() => setAccion('ASIGNAR_ROL')}
          />
          Asignarle otro rol en este club
        </label>
        {accion === 'ASIGNAR_ROL' && (
          <Campo
            etiqueta="Rol nuevo"
            valor={rolNuevo}
            alCambiar={(valor) => setRolNuevo(valor as RolNuevo)}
            opciones={[
              { valor: 'DIRECTIVO', texto: 'Directivo' },
              { valor: 'ENTRENADOR', texto: 'Entrenador' },
            ]}
          />
        )}
        <label className="fila">
          <input
            type="radio"
            name="accion"
            checked={accion === 'ELIMINAR_DEL_CLUB'}
            onChange={() => setAccion('ELIMINAR_DEL_CLUB')}
          />
          Eliminarlo del club por completo
        </label>
        {accion === 'ELIMINAR_DEL_CLUB' && (
          <p className="texto-suave">
            Dejará de poder entrar a este club. Si no pertenece a ningún otro, su cuenta se elimina.
          </p>
        )}
      </DialogoConfirmacion>
    </Tarjeta>
  );
}
