import { useEffect, useState } from 'react';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { api } from '../../compartido/api/cliente';
import { ErrorApi } from '../../compartido/api/errores';
import type { CategoriaDetalleDto } from '../../compartido/api/tiposCategorias';
import { useCarga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { EtiquetaEstado } from '../../compartido/componentes/EtiquetaEstado';
import { useClub } from '../contextoClub';
import { AccionesDeCategoria } from './AccionesDeCategoria';
import { SeccionEntrenadores } from './SeccionEntrenadores';
import { SeccionEquipos } from './SeccionEquipos';
import { SeccionJugadores } from './SeccionJugadores';
import { cuantosJugadores, puedeVerCategorias } from './textos';
import type { Mensaje } from './useAccion';

/**
 * Detalle de una categoría: su año, su estado y tres secciones, entrenadores, equipos y jugadores.
 * El presidente la gestiona desde aquí; el directivo y el entrenador que la tiene asignada solo la
 * consultan.
 */
export function DetalleCategoria() {
  const { club } = useClub();

  if (!puedeVerCategorias(club.miRol)) {
    return <Navigate to={`/club/${club.clubId}`} replace />;
  }

  return <CategoriaDelClub clubId={club.clubId} esPresidente={club.miRol === 'PRESIDENTE'} />;
}

function CategoriaDelClub({ clubId, esPresidente }: { clubId: string; esPresidente: boolean }) {
  const { categoriaId = '' } = useParams();
  const navegar = useNavigate();
  const lista = `/club/${clubId}/categorias`;
  const [mensaje, setMensaje] = useState<Mensaje | null>(null);
  const detalle = useCarga(`${clubId}/categorias/${categoriaId}`, () =>
    api.get<CategoriaDetalleDto>(`/api/clubes/${clubId}/categorias/${categoriaId}`),
  );
  const categoria = detalle.datos;
  const noExiste = detalle.fallo instanceof ErrorApi && detalle.fallo.status === 404;
  const secciones = { clubId, esPresidente, alCambiar: detalle.fijar };

  // La API responde lo mismo si la categoría no existe o si ya no es de quien pregunta (por
  // ejemplo, un entrenador al que se la retiraron con la pantalla abierta): se vuelve a la lista.
  useEffect(() => {
    if (noExiste) {
      navegar(lista, { replace: true, state: { aviso: 'Esa categoría ya no está entre las que puedes ver.' } });
    }
  }, [noExiste, navegar, lista]);

  return (
    <>
      <p>
        <Link to={lista}>← Categorías</Link>
      </p>
      {detalle.error && !noExiste && <Aviso tono="error">{detalle.error}</Aviso>}
      {!categoria && detalle.cargando && <p className="texto-suave">Cargando…</p>}

      {categoria && (
        <>
          <div className="cabecera">
            <h1>Categoría {categoria.anio}</h1>
            <span className="fila">
              {categoria.activa ? (
                <EtiquetaEstado tono="exito" texto="Activa" />
              ) : (
                <EtiquetaEstado tono="neutro" texto="Inactiva" />
              )}
              <span className="texto-suave">{cuantosJugadores(categoria.numeroJugadores)}</span>
            </span>
          </div>

          {esPresidente && (
            <AccionesDeCategoria
              clubId={clubId}
              categoria={categoria}
              alTerminar={(resultado) => {
                setMensaje(resultado);
                detalle.recargar();
              }}
              alBorrar={() => navegar(lista, { replace: true })}
            />
          )}
          {mensaje && <Aviso tono={mensaje.tono}>{mensaje.texto}</Aviso>}

          <SeccionEntrenadores {...secciones} categoria={categoria} />
          <SeccionEquipos {...secciones} categoria={categoria} />
          <SeccionJugadores {...secciones} categoria={categoria} alRecargar={detalle.recargar} />
        </>
      )}
    </>
  );
}
