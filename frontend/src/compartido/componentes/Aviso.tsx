import type { ReactNode } from 'react';

export type Tono = 'info' | 'exito' | 'aviso' | 'error' | 'neutro';

interface Props {
  tono?: Tono;
  children: ReactNode;
}

/** Mensaje destacado. Los errores se anuncian a los lectores de pantalla. */
export function Aviso({ tono = 'info', children }: Props) {
  return (
    <div className={`aviso tono-${tono}`} role={tono === 'error' ? 'alert' : 'status'}>
      {children}
    </div>
  );
}
