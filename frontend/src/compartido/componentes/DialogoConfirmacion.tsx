import { useEffect, useRef, type FormEvent, type ReactNode } from 'react';
import { Aviso } from './Aviso';
import { Boton } from './Boton';

interface Props {
  abierto: boolean;
  titulo: string;
  textoConfirmar: string;
  /** Marca la acción como destructiva. */
  peligro?: boolean;
  /** Impide confirmar, por ejemplo mientras falta elegir una opción. */
  deshabilitado?: boolean;
  cargando?: boolean;
  error?: string;
  alConfirmar: () => void;
  alCancelar: () => void;
  children: ReactNode;
}

/** Diálogo modal que pide confirmar una acción antes de hacerla. */
export function DialogoConfirmacion({
  abierto,
  titulo,
  textoConfirmar,
  peligro = false,
  deshabilitado = false,
  cargando = false,
  error,
  alConfirmar,
  alCancelar,
  children,
}: Props) {
  const dialogo = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const elemento = dialogo.current;
    if (!elemento) {
      return;
    }

    if (abierto && !elemento.open) {
      elemento.showModal();
    } else if (!abierto && elemento.open) {
      elemento.close();
    }
  }, [abierto]);

  function enviar(evento: FormEvent) {
    evento.preventDefault();
    alConfirmar();
  }

  return (
    <dialog
      ref={dialogo}
      className="dialogo"
      onCancel={(evento) => {
        evento.preventDefault();
        alCancelar();
      }}
    >
      {abierto && (
        <form onSubmit={enviar}>
          <h2>{titulo}</h2>
          <div className="dialogo-cuerpo">{children}</div>
          {error && <Aviso tono="error">{error}</Aviso>}
          <div className="dialogo-acciones">
            <Boton variante="secundario" onClick={alCancelar} disabled={cargando}>
              Cancelar
            </Boton>
            <Boton
              type="submit"
              variante={peligro ? 'peligro' : 'principal'}
              disabled={deshabilitado}
              cargando={cargando}
            >
              {textoConfirmar}
            </Boton>
          </div>
        </form>
      )}
    </dialog>
  );
}
