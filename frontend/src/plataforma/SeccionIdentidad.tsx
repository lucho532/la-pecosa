import { useId, useState, type ChangeEvent, type FormEvent } from 'react';
import { api } from '../compartido/api/cliente';
import { mensajeDe } from '../compartido/api/errores';
import type { ActualizarColoresDto, ClubDetalleDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Boton } from '../compartido/componentes/Boton';
import { Campo } from '../compartido/componentes/Campo';
import { Tarjeta } from '../compartido/componentes/Tarjeta';
import { temasDondeSePierde, type Tema } from '../compartido/tema/contraste';
import { Escudo, IdentidadClub } from '../compartido/tema/IdentidadClub';
import type { PropsSeccion } from './DetalleClub';

const NOMBRE_DEL_TEMA: Record<Tema, string> = { claro: 'claro', oscuro: 'oscuro' };

/** Texto del aviso de contraste de un color, o `null` si se distingue en los dos temas. */
function avisoDeContraste(nombre: string, color: string): string | null {
  const temas = temasDondeSePierde(color);
  if (temas.length === 0) {
    return null;
  }

  return `El color ${nombre} casi no se distingue del fondo en el tema ${temas.map((tema) => NOMBRE_DEL_TEMA[tema]).join(' ni en el ')}.`;
}

/**
 * Identidad visual de un club: escudo y colores. Solo la cambia el DESARROLLADOR. Antes de guardar
 * avisa si un color se perdería en algún tema; es un aviso y no impide guardar (RF-009).
 */
export function SeccionIdentidad({ club, alActualizar }: PropsSeccion) {
  const base = `/api/plataforma/clubes/${club.clubId}`;
  const [principal, setPrincipal] = useState(club.identidad.colorPrincipal ?? '#231F1E');
  const [acento, setAcento] = useState(club.identidad.colorAcento ?? '#5E5650');
  const idEscudo = useId();
  const [ocupado, setOcupado] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hecho, setHecho] = useState<string | null>(null);

  const avisos = [avisoDeContraste('principal', principal), avisoDeContraste('de acento', acento)].filter(
    (aviso): aviso is string => aviso !== null,
  );
  const vistaPrevia = { colorPrincipal: principal, colorAcento: acento, urlEscudo: club.identidad.urlEscudo };

  async function ejecutar(accion: () => Promise<ClubDetalleDto>, mensaje: string) {
    setError(null);
    setHecho(null);
    setOcupado(true);
    try {
      alActualizar(await accion());
      setHecho(mensaje);
    } catch (fallo) {
      setError(mensajeDe(fallo));
    } finally {
      setOcupado(false);
    }
  }

  function guardarColores(evento: FormEvent) {
    evento.preventDefault();
    const datos: ActualizarColoresDto = { colorPrincipal: principal, colorAcento: acento };
    void ejecutar(() => api.put<ClubDetalleDto>(`${base}/colores`, datos), 'Colores guardados.');
  }

  function cargarEscudo(evento: ChangeEvent<HTMLInputElement>) {
    const archivo = evento.target.files?.[0];
    evento.target.value = '';
    if (archivo) {
      void ejecutar(() => api.putArchivo<ClubDetalleDto>(`${base}/escudo`, archivo), 'Escudo guardado.');
    }
  }

  return (
    <Tarjeta titulo="Identidad">
      {error && <Aviso tono="error">{error}</Aviso>}
      {hecho && <Aviso tono="exito">{hecho}</Aviso>}

      <div className="fila">
        <Escudo identidad={club.identidad} nombre={club.nombre} />
        <div className="campo">
          <label htmlFor={idEscudo}>{club.identidad.urlEscudo ? 'Cambiar el escudo' : 'Cargar el escudo'}</label>
          <input
            id={idEscudo}
            type="file"
            accept="image/png,image/jpeg,image/webp"
            onChange={cargarEscudo}
            disabled={ocupado}
          />
          <span className="texto-suave">PNG, JPEG o WebP de hasta 1 MB.</span>
        </div>
      </div>

      <form className="columna" onSubmit={guardarColores}>
        <div className="rejilla">
          <Campo etiqueta="Color principal" type="color" valor={principal} alCambiar={setPrincipal} />
          <Campo etiqueta="Color de acento" type="color" valor={acento} alCambiar={setAcento} />
        </div>

        <div className="rejilla" aria-label="Vista previa en los dos temas">
          {(['claro', 'oscuro'] as Tema[]).map((tema) => (
            <div key={tema} data-tema={tema} className="tarjeta" style={{ background: 'var(--fondo)', color: 'var(--texto)' }}>
              <span className="texto-suave">Tema {NOMBRE_DEL_TEMA[tema]}</span>
              <IdentidadClub identidad={vistaPrevia} className="fila">
                <span className="boton boton-principal">{club.nombre}</span>
                <span
                  className="etiqueta"
                  style={{ background: 'var(--color-club-acento)', color: 'var(--color-club-acento-texto)' }}
                >
                  Acento
                </span>
              </IdentidadClub>
            </div>
          ))}
        </div>

        {avisos.map((aviso) => (
          <Aviso key={aviso} tono="aviso">
            {aviso} Puedes guardarlo igualmente.
          </Aviso>
        ))}

        <div className="fila">
          <Boton type="submit" cargando={ocupado} textoCargando="Guardando…">
            Guardar colores
          </Boton>
        </div>
      </form>
    </Tarjeta>
  );
}
