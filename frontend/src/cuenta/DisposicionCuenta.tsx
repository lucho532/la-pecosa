import type { ReactNode } from 'react';
import type { IdentidadClubDto } from '../compartido/api/tipos';
import { Escudo, IdentidadClub } from '../compartido/tema/IdentidadClub';

interface Props {
  titulo: string;
  /** Texto bajo el título, por ejemplo el nombre del club que invita. */
  subtitulo?: string;
  /** Identidad del club que invita. Sin ella se usa la identidad neutra de la plataforma. */
  identidad?: IdentidadClubDto;
  children: ReactNode;
}

/** Disposición común de las pantallas sin sesión: entrar, recuperar, restablecer e invitación. */
export function DisposicionCuenta({ titulo, subtitulo, identidad, children }: Props) {
  return (
    <IdentidadClub identidad={identidad} className="centrado">
      <header className="columna">
        <p className="lateral-marca" style={{ padding: 0 }}>
          La Pecosa
        </p>
        {identidad && <Escudo identidad={identidad} nombre={titulo} />}
        <h1>{titulo}</h1>
        {subtitulo && <p className="texto-suave">{subtitulo}</p>}
      </header>
      <main className="tarjeta">{children}</main>
    </IdentidadClub>
  );
}
