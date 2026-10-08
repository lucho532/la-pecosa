import { useState } from 'react';
import type { IngresoEnEsperaDto, Rol, RolDeIngreso } from '../../compartido/api/tipos';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';

interface Opcion {
  rol: RolDeIngreso;
  texto: string;
  ayuda: string;
}

const OPCIONES: Opcion[] = [
  { rol: 'JUGADOR', texto: 'Jugador', ayuda: 'La cuenta es de un jugador del club; la usa su familia.' },
  { rol: 'ENTRENADOR', texto: 'Entrenador', ayuda: 'Profesor del club.' },
  { rol: 'DIRECTIVO', texto: 'Directivo', ayuda: 'Miembro de la directiva. Podrá invitar y aprobar ingresos.' },
];

/**
 * Roles que puede dejar al aprobar cada quien: el presidente, los tres; un directivo, jugador o
 * entrenador. Es una ayuda de pantalla: quien decide es la API (RF-022).
 */
function opcionesPara(miRol: Rol): Opcion[] {
  return miRol === 'PRESIDENTE' ? OPCIONES : OPCIONES.filter((opcion) => opcion.rol !== 'DIRECTIVO');
}

interface Props {
  /** Persona cuyo ingreso se va a aprobar. */
  persona: IngresoEnEsperaDto;
  /** Rol en el club de quien aprueba. */
  miRol: Rol;
  cargando: boolean;
  error?: string;
  alConfirmar: (rol: RolDeIngreso) => void;
  alCancelar: () => void;
}

/**
 * Diálogo para aprobar un ingreso: muestra a quién se aprueba y pide elegir cómo entra al club. El
 * rol elegido será su único rol. Se monta al abrirlo, así que siempre empieza en "Jugador".
 */
export function DialogoAprobarIngreso({ persona, miRol, cargando, error, alConfirmar, alCancelar }: Props) {
  const [rol, setRol] = useState<RolDeIngreso>('JUGADOR');

  return (
    <DialogoConfirmacion
      abierto
      titulo={`Aprobar el ingreso de ${persona.nombres} ${persona.apellidos}`}
      textoConfirmar="Aprobar ingreso"
      cargando={cargando}
      error={error}
      alConfirmar={() => alConfirmar(rol)}
      alCancelar={alCancelar}
    >
      <fieldset className="opciones">
        <legend>¿Cómo entra al club?</legend>
        {opcionesPara(miRol).map((opcion) => (
          <label key={opcion.rol} className="opcion">
            <input
              type="radio"
              name="rol-de-ingreso"
              checked={rol === opcion.rol}
              onChange={() => setRol(opcion.rol)}
            />
            <span>
              <strong>{opcion.texto}</strong>
              <span className="texto-suave"> {opcion.ayuda}</span>
            </span>
          </label>
        ))}
      </fieldset>
      <p className="texto-suave">Al aprobar, esta persona entra a la aplicación del club con ese rol.</p>
    </DialogoConfirmacion>
  );
}
