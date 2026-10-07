import type { ReactNode } from 'react';

interface Props {
  titulo: string;
  /** Texto bajo el título, por ejemplo el nombre del club que invita. */
  subtitulo?: string;
  children: ReactNode;
}

/** Disposición común de las pantallas sin sesión: entrar, recuperar, restablecer e invitación. */
export function DisposicionCuenta({ titulo, subtitulo, children }: Props) {
  return (
    <div className="centrado">
      <header className="columna">
        <p className="lateral-marca" style={{ padding: 0 }}>
          La Pecosa
        </p>
        <h1>{titulo}</h1>
        {subtitulo && <p className="texto-suave">{subtitulo}</p>}
      </header>
      <main className="tarjeta">{children}</main>
    </div>
  );
}
