import type { ReactNode } from 'react';

export interface Columna<Fila> {
  titulo: string;
  celda: (fila: Fila) => ReactNode;
}

interface Props<Fila> {
  /** Descripción de la tabla para los lectores de pantalla. */
  descripcion: string;
  columnas: Columna<Fila>[];
  filas: Fila[];
  clave: (fila: Fila) => string;
  /** Lo que se muestra cuando no hay filas. */
  vacio: ReactNode;
}

/** Tabla de datos. Si no cabe, se desplaza dentro de su caja y no la página. */
export function Tabla<Fila>({ descripcion, columnas, filas, clave, vacio }: Props<Fila>) {
  if (filas.length === 0) {
    return <p className="texto-suave">{vacio}</p>;
  }

  return (
    <div className="tabla-caja">
      <table className="tabla">
        <caption className="solo-lectores">{descripcion}</caption>
        <thead>
          <tr>
            {columnas.map((columna) => (
              <th key={columna.titulo} scope="col">
                {columna.titulo}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filas.map((fila) => (
            <tr key={clave(fila)}>
              {columnas.map((columna) => (
                <td key={columna.titulo}>{columna.celda(fila)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
