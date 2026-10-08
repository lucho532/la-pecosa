import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { mensajeDe } from '../../compartido/api/errores';
import type { JugadorDeCategoriaDto } from '../../compartido/api/tiposCategorias';
import { Boton } from '../../compartido/componentes/Boton';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { nombreCompleto } from './textos';

interface Props {
  clubId: string;
  jugador: JugadorDeCategoriaDto;
  deshabilitado?: boolean;
  /** Se llama cuando el jugador quedó retirado, con lo que hay que decir. */
  alRetirar: (texto: string) => void;
}

/**
 * Botón con el que el presidente retira del club a un jugador que se fue. Siempre pide
 * confirmación antes (RF-041) y explica qué pasa: el jugador sale de su categoría y de sus
 * equipos y deja de entrar a este club, pero sus datos se conservan y se le puede reincorporar.
 */
export function BotonRetirarJugador({ clubId, jugador, deshabilitado = false, alRetirar }: Props) {
  const [abierto, setAbierto] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState<string | undefined>();

  async function retirar() {
    setEnviando(true);
    setError(undefined);
    try {
      await api.post(`/api/clubes/${clubId}/jugadores/${jugador.usuarioRolId}/retiro`);
      setAbierto(false);
      alRetirar(`${nombreCompleto(jugador)} quedó retirado del club. Puedes reincorporarlo desde "Retirados".`);
    } catch (fallo) {
      setError(mensajeDe(fallo));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <>
      <Boton
        variante="secundario"
        disabled={deshabilitado}
        onClick={() => {
          setError(undefined);
          setAbierto(true);
        }}
      >
        Retirar
      </Boton>
      <DialogoConfirmacion
        abierto={abierto}
        titulo={`Retirar del club a ${nombreCompleto(jugador)}`}
        textoConfirmar="Retirar del club"
        peligro
        cargando={enviando}
        error={error}
        alConfirmar={() => void retirar()}
        alCancelar={() => setAbierto(false)}
      >
        <p>
          Saldrá de su categoría y de sus equipos, dejará de aparecer en las listas de jugadores y su cuenta ya no
          entrará a este club.
        </p>
        <p className="texto-suave">
          Sus datos se conservan y podrás reincorporarlo desde "Retirados". No se le envía ningún aviso.
        </p>
      </DialogoConfirmacion>
    </>
  );
}
