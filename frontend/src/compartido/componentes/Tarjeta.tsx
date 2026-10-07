import type { ReactNode } from 'react';

interface Props {
  titulo?: string;
  /** Botones o enlaces que acompañan al título. */
  acciones?: ReactNode;
  children: ReactNode;
}

/** Caja con borde que agrupa una sección de una pantalla. */
export function Tarjeta({ titulo, acciones, children }: Props) {
  return (
    <section className="tarjeta">
      {(titulo || acciones) && (
        <div className="tarjeta-cabecera">
          {titulo && <h2>{titulo}</h2>}
          {acciones}
        </div>
      )}
      {children}
    </section>
  );
}
