import { api } from '../compartido/api/cliente';
import type { MiCategoriaDto } from '../compartido/api/tiposCategorias';
import { useCarga } from '../compartido/api/useCarga';
import { Aviso } from '../compartido/componentes/Aviso';
import { Tarjeta } from '../compartido/componentes/Tarjeta';

/**
 * Tarjeta "Mi categoría" del inicio del club, solo para la cuenta de un jugador: su categoría, los
 * equipos en los que está y quiénes lo entrenan, con el equipo que dirige cada uno (RF-035). De un
 * entrenador solo se muestra el nombre: la API no entrega nada más.
 */
export function TarjetaMiCategoria({ clubId }: { clubId: string }) {
  const mia = useCarga(`${clubId}/mi-categoria`, () => api.get<MiCategoriaDto>(`/api/clubes/${clubId}/mi-categoria`));
  const categoria = mia.datos?.categoria;

  return (
    <Tarjeta titulo="Mi categoría">
      {mia.error && <Aviso tono="error">{mia.error}</Aviso>}
      {!mia.datos && mia.cargando && <p className="texto-suave">Cargando…</p>}
      {mia.datos && !categoria && (
        <p>
          Todavía no tienes categoría asignada. El club te ubicará en una; no tienes que hacer nada.
        </p>
      )}
      {categoria && (
        <>
          <p>
            <strong>Categoría {categoria.anio}</strong>
          </p>
          <p>
            {categoria.equipos.length === 0
              ? 'Sin equipo por ahora.'
              : `${categoria.equipos.length === 1 ? 'Equipo' : 'Equipos'}: ${categoria.equipos.join(', ')}`}
          </p>
          {categoria.entrenadores.length === 0 ? (
            <p className="texto-suave">La categoría todavía no tiene entrenador.</p>
          ) : (
            <>
              <p className="texto-suave">{categoria.entrenadores.length === 1 ? 'Entrenador' : 'Entrenadores'}</p>
              <ul>
                {categoria.entrenadores.map((entrenador, indice) => (
                  <li key={`${entrenador.nombres}-${entrenador.apellidos}-${indice}`}>
                    {entrenador.nombres} {entrenador.apellidos}
                    {entrenador.equipos.length > 0 && (
                      <span className="texto-suave"> · dirige {entrenador.equipos.join(', ')}</span>
                    )}
                  </li>
                ))}
              </ul>
            </>
          )}
        </>
      )}
    </Tarjeta>
  );
}
