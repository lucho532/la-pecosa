import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import type { CandidatoEntrenadorDto } from '../../compartido/api/tiposCategorias';
import { useCarga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { nombreDeRol } from '../../compartido/formato';
import { nombreCompleto } from './textos';

interface Props {
  clubId: string;
  categoriaId: string;
  anio: number;
  cargando: boolean;
  error?: string;
  alConfirmar: (candidato: CandidatoEntrenadorDto) => void;
  alCancelar: () => void;
}

/**
 * Diálogo para asignar un entrenador a una categoría. Ofrece a los integrantes aprobados del club
 * con rol entrenador, directivo o presidente que todavía no están asignados a ella, incluido quien
 * lo abre (RF-017). La lista la decide la API; se pide cada vez que se abre.
 */
export function DialogoAsignarEntrenador({ clubId, categoriaId, anio, cargando, error, alConfirmar, alCancelar }: Props) {
  const [elegido, setElegido] = useState<CandidatoEntrenadorDto | null>(null);
  const candidatos = useCarga(`${clubId}/categorias/${categoriaId}/candidatos`, () =>
    api.get<CandidatoEntrenadorDto[]>(`/api/clubes/${clubId}/categorias/${categoriaId}/entrenadores/candidatos`),
  );

  return (
    <DialogoConfirmacion
      abierto
      titulo={`Asignar un entrenador a la categoría ${anio}`}
      textoConfirmar="Asignar entrenador"
      deshabilitado={elegido === null}
      cargando={cargando}
      error={error}
      alConfirmar={() => {
        if (elegido) {
          alConfirmar(elegido);
        }
      }}
      alCancelar={alCancelar}
    >
      {candidatos.error && <Aviso tono="error">{candidatos.error}</Aviso>}
      {!candidatos.datos && candidatos.cargando && <p className="texto-suave">Cargando…</p>}
      {candidatos.datos?.length === 0 && (
        <p className="texto-suave">
          No queda nadie por asignar: todos los entrenadores, directivos y presidentes del club ya entrenan esta
          categoría.
        </p>
      )}
      {candidatos.datos && candidatos.datos.length > 0 && (
        <fieldset className="opciones">
          <legend>¿Quién la entrena?</legend>
          {candidatos.datos.map((candidato) => (
            <label key={candidato.usuarioRolId} className="opcion">
              <input
                type="radio"
                name="candidato-a-entrenador"
                checked={elegido?.usuarioRolId === candidato.usuarioRolId}
                onChange={() => setElegido(candidato)}
              />
              <span>
                <strong>{nombreCompleto(candidato)}</strong>
                <span className="texto-suave"> {nombreDeRol(candidato.rol)}</span>
              </span>
            </label>
          ))}
        </fieldset>
      )}
      <p className="texto-suave">Asignarlo no cambia su rol en el club: solo lo muestra como entrenador de la categoría.</p>
    </DialogoConfirmacion>
  );
}
