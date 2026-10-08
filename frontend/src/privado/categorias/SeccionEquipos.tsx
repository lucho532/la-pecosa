import { useState, type FormEvent } from 'react';
import { api } from '../../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { CategoriaDetalleDto, EquipoDto, NombreEquipoDto } from '../../compartido/api/tiposCategorias';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { Campo } from '../../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { Tabla, type Columna } from '../../compartido/componentes/Tabla';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { cuantosJugadores, nombreCompleto } from './textos';
import { useAccion } from './useAccion';

interface Props {
  clubId: string;
  categoria: CategoriaDetalleDto;
  esPresidente: boolean;
  /** Recibe la categoría tal como la devolvió la API tras el cambio. */
  alCambiar: (categoria: CategoriaDetalleDto) => void;
}

type Pendiente = { accion: 'renombrar' | 'desactivar' | 'borrar'; equipo: EquipoDto } | null;

const TITULOS = { renombrar: 'Cambiar el nombre de', desactivar: 'Desactivar', borrar: 'Borrar' } as const;
const CONFIRMAR = { renombrar: 'Guardar el nombre', desactivar: 'Desactivar el equipo', borrar: 'Borrar el equipo' } as const;

/**
 * Equipos de una categoría (A y B, élite y B): cada uno con sus jugadores y quién lo dirige. El
 * presidente los crea, los renombra, los desactiva y borra los que nunca se usaron. Una categoría
 * sin equipos funciona igual (RF-022).
 */
export function SeccionEquipos({ clubId, categoria, esPresidente, alCambiar }: Props) {
  const base = `/api/clubes/${clubId}/categorias/${categoria.categoriaId}/equipos`;
  const { ocupado, mensaje, ejecutar } = useAccion();
  const [nombre, setNombre] = useState('');
  const [errorNombre, setErrorNombre] = useState<string | undefined>();
  const [pendiente, setPendiente] = useState<Pendiente>(null);
  const [nombreNuevo, setNombreNuevo] = useState('');
  const [errorDialogo, setErrorDialogo] = useState<string | undefined>();

  async function crear(evento: FormEvent) {
    evento.preventDefault();
    setErrorNombre(undefined);
    await ejecutar(async () => {
      try {
        const datos: NombreEquipoDto = { nombre };
        alCambiar(await api.post<CategoriaDetalleDto>(base, datos));
        setNombre('');
        return null;
      } catch (fallo) {
        if (fallo instanceof ErrorApi && fallo.errorDe('nombre')) {
          setErrorNombre(fallo.errorDe('nombre'));
          return null;
        }

        throw fallo;
      }
    });
  }

  function abrir(accion: NonNullable<Pendiente>['accion'], equipo: EquipoDto) {
    setErrorDialogo(undefined);
    setNombreNuevo(equipo.nombre);
    setPendiente({ accion, equipo });
  }

  /** Ejecuta la acción del diálogo abierto; si la API la rechaza, el error se queda en el diálogo. */
  async function confirmar() {
    if (!pendiente) {
      return;
    }

    const { accion, equipo } = pendiente;
    const ruta = `${base}/${equipo.equipoId}`;
    setErrorDialogo(undefined);
    await ejecutar(async () => {
      try {
        const datos: NombreEquipoDto = { nombre: nombreNuevo };
        alCambiar(
          accion === 'renombrar'
            ? await api.put<CategoriaDetalleDto>(ruta, datos)
            : accion === 'desactivar'
              ? await api.post<CategoriaDetalleDto>(`${ruta}/desactivacion`)
              : await api.delete<CategoriaDetalleDto>(ruta),
        );
        setPendiente(null);
      } catch (fallo) {
        setErrorDialogo(fallo instanceof ErrorApi ? (fallo.errorDe('nombre') ?? fallo.title) : mensajeDe(fallo));
      }

      return null;
    });
  }

  const columnas: Columna<EquipoDto>[] = [
    { titulo: 'Equipo', celda: (equipo) => <strong>{equipo.nombre}</strong> },
    { titulo: 'Jugadores', celda: (equipo) => cuantosJugadores(equipo.numeroJugadores) },
    {
      titulo: 'Lo dirige',
      celda: (equipo) =>
        categoria.entrenadores
          .filter((entrenador) => entrenador.equipos.some((suyo) => suyo.equipoId === equipo.equipoId))
          .map(nombreCompleto)
          .join(', ') || 'Nadie todavía',
    },
  ];

  if (esPresidente) {
    columnas.push({
      titulo: 'Acciones',
      celda: (equipo) => (
        <span className="fila">
          <Boton variante="secundario" onClick={() => abrir('renombrar', equipo)} disabled={ocupado}>
            Renombrar
          </Boton>
          <Boton variante="secundario" onClick={() => abrir('desactivar', equipo)} disabled={ocupado}>
            Desactivar
          </Boton>
          {equipo.sePuedeBorrar && (
            <Boton variante="secundario" onClick={() => abrir('borrar', equipo)} disabled={ocupado}>
              Borrar
            </Boton>
          )}
        </span>
      ),
    });
  }

  return (
    <Tarjeta titulo="Equipos">
      {esPresidente && categoria.activa && (
        <form className="fila" onSubmit={(evento) => void crear(evento)} noValidate>
          <Campo
            etiqueta="Nombre del equipo nuevo"
            valor={nombre}
            alCambiar={setNombre}
            error={errorNombre}
            placeholder="A, B, Élite…"
            autoComplete="off"
            maxLength={30}
          />
          <Boton type="submit" cargando={ocupado && pendiente === null} textoCargando="Creando…">
            Crear equipo
          </Boton>
        </form>
      )}
      {mensaje && <Aviso tono={mensaje.tono}>{mensaje.texto}</Aviso>}
      <Tabla
        descripcion={`Equipos de la categoría ${categoria.anio}`}
        columnas={columnas}
        filas={categoria.equipos}
        clave={(equipo) => equipo.equipoId}
        vacio="Esta categoría no está dividida en equipos."
      />

      <DialogoConfirmacion
        abierto={pendiente !== null}
        titulo={pendiente ? `${TITULOS[pendiente.accion]} el equipo ${pendiente.equipo.nombre}` : ''}
        textoConfirmar={pendiente ? CONFIRMAR[pendiente.accion] : ''}
        peligro={pendiente?.accion !== 'renombrar'}
        cargando={ocupado}
        error={errorDialogo}
        alConfirmar={() => void confirmar()}
        alCancelar={() => setPendiente(null)}
      >
        {pendiente?.accion === 'renombrar' && (
          <Campo etiqueta="Nombre del equipo" valor={nombreNuevo} alCambiar={setNombreNuevo} maxLength={30} autoComplete="off" />
        )}
        {pendiente?.accion === 'desactivar' && (
          <p>
            El equipo dejará de mostrarse y no se puede reactivar. Sus jugadores siguen en la categoría y sus
            entrenadores siguen asignados a ella.
          </p>
        )}
        {pendiente?.accion === 'borrar' && (
          <p>El equipo se borrará del todo. Se puede borrar porque nunca ha tenido jugadores ni entrenadores.</p>
        )}
      </DialogoConfirmacion>
    </Tarjeta>
  );
}
