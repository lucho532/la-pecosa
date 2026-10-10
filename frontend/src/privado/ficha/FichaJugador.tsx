import { Link, useParams } from 'react-router-dom';
import { api } from '../../compartido/api/cliente';
import { ErrorApi } from '../../compartido/api/errores';
import type { ActualizarFichaDto, FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { useCarga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { nombreCompleto, puedeVerCategorias } from '../categorias/textos';
import { useClub } from '../contextoClub';
import { FichaDeSoloLectura } from './FichaDeSoloLectura';
import { FormularioFicha } from './FormularioFicha';
import { SeccionDocumentos } from './SeccionDocumentos';
import { SeccionIdentidad } from './SeccionIdentidad';
import { UltimoCambio } from './UltimoCambio';

/**
 * Pantalla de la ficha de un jugador, la misma para todos los roles. Carga la ficha y la reparte en
 * secciones. Muestra y permite solo lo que trae la respuesta: los grupos que la API no entrega no
 * se pintan, y los botones dependen de `permisos`, no del rol. Quien no puede ver la ficha recibe
 * de la API el mismo "no encontrado" que si no existiera, y aquí se le dice lo mismo.
 */
export function FichaJugador() {
  const { club } = useClub();
  const { usuarioRolId = '' } = useParams();
  const ruta = `/api/clubes/${club.clubId}/jugadores/${usuarioRolId}/ficha`;
  const carga = useCarga(`${club.clubId}/ficha/${usuarioRolId}`, () => api.get<FichaJugadorDto>(ruta));
  const ficha = carga.datos;
  const noLaVe = carga.fallo instanceof ErrorApi && carga.fallo.status === 404;

  // Quien ve las listas del club vuelve a la categoría del jugador; la familia, al inicio.
  const volver = !puedeVerCategorias(club.miRol)
    ? { a: `/club/${club.clubId}`, texto: 'Inicio' }
    : ficha?.categoriaId
      ? { a: `/club/${club.clubId}/categorias/${ficha.categoriaId}`, texto: `Categoría ${ficha.categoriaAnio}` }
      : { a: `/club/${club.clubId}/categorias`, texto: 'Categorías' };

  async function guardar(datos: ActualizarFichaDto): Promise<FichaJugadorDto> {
    const guardada = await api.put<FichaJugadorDto>(ruta, datos);
    carga.fijar(guardada);
    return guardada;
  }

  return (
    <>
      <Link to={volver.a}>← {volver.texto}</Link>
      {noLaVe ? (
        <Aviso tono="aviso">Esta ficha no existe o no puedes verla.</Aviso>
      ) : (
        carga.error && <Aviso tono="error">{carga.error}</Aviso>
      )}
      {!ficha && carga.cargando && <p className="texto-suave">Cargando…</p>}
      {ficha && (
        <>
          <div className="columna">
            <h1>{ficha.usuarioRolId === club.miUsuarioRolId ? 'Mi ficha' : `Ficha de ${nombreCompleto(ficha)}`}</h1>
            <UltimoCambio cambio={ficha.ultimoCambio} />
          </div>
          <SeccionIdentidad ficha={ficha} rutaDeLaFicha={ruta} alCambiar={carga.fijar} />
          {ficha.permisos.puedeCambiar && ficha.datosClinicos ? (
            <FormularioFicha key={ficha.usuarioRolId} ficha={ficha} alGuardar={guardar} />
          ) : (
            <FichaDeSoloLectura ficha={ficha} />
          )}
          {ficha.documentos && (
            <SeccionDocumentos
              rutaDeLaFicha={ruta}
              documentos={ficha.documentos}
              puedeCambiar={ficha.permisos.puedeCambiar}
              alCambiar={carga.fijar}
            />
          )}
        </>
      )}
    </>
  );
}
