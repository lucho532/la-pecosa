import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { TipoDocumento } from '../../compartido/api/tipos';
import type { CambiarDocumentoIdentidadDto, FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { Campo } from '../../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';
import { nombreDeTipoDocumento } from '../../compartido/formato';

const TIPOS: TipoDocumento[] = ['REGISTRO_CIVIL', 'TARJETA_IDENTIDAD', 'CEDULA_CIUDADANIA', 'CEDULA_EXTRANJERIA'];

interface Props {
  ficha: FichaJugadorDto;
  /** Dirección de la ficha en la API. */
  rutaDeLaFicha: string;
  /** Recibe la ficha tal como la devolvió la API con el documento nuevo. */
  alGuardar: (ficha: FichaJugadorDto) => void;
  alCancelar: () => void;
}

/**
 * Diálogo para cambiar el tipo y el número del documento de identidad del jugador, por ejemplo al
 * pasar de registro civil a tarjeta de identidad. Sigue siendo el mismo jugador, con su categoría y
 * su ficha. Los errores de un campo se pintan junto a él; un número que ya está en uso, como aviso.
 */
export function DialogoCambiarDocumento({ ficha, rutaDeLaFicha, alGuardar, alCancelar }: Props) {
  const [tipo, setTipo] = useState<string>(ficha.tipoDocumento);
  const [numero, setNumero] = useState(ficha.numeroDocumento);
  const [errores, setErrores] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string>();
  const [enviando, setEnviando] = useState(false);

  async function guardar() {
    setError(undefined);
    setErrores({});
    setEnviando(true);
    try {
      const datos: CambiarDocumentoIdentidadDto = { tipoDocumento: tipo as TipoDocumento, numeroDocumento: numero };
      alGuardar(await api.put<FichaJugadorDto>(`${rutaDeLaFicha}/documento-identidad`, datos));
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.codigo === 'datos_invalidos') {
        setErrores(fallo.errores);
      } else {
        setError(`${mensajeDe(fallo)} No se cambió nada.`);
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <DialogoConfirmacion
      abierto
      titulo="Cambiar el documento de identidad"
      textoConfirmar="Guardar documento"
      deshabilitado={numero.trim() === ''}
      cargando={enviando}
      error={error}
      alConfirmar={() => void guardar()}
      alCancelar={alCancelar}
    >
      <p className="texto-suave">
        Sigue siendo el mismo jugador: conserva su categoría, sus equipos y su ficha. Desde que se guarde, para
        iniciar sesión con el documento se usa el número nuevo.
      </p>
      <Campo
        etiqueta="Tipo de documento"
        valor={tipo}
        alCambiar={setTipo}
        error={errores.tipoDocumento?.[0]}
        opciones={TIPOS.map((valor) => ({ valor, texto: nombreDeTipoDocumento(valor) }))}
      />
      <Campo
        etiqueta="Número de documento"
        valor={numero}
        alCambiar={setNumero}
        error={errores.numeroDocumento?.[0]}
        maxLength={30}
        inputMode="numeric"
        required
      />
    </DialogoConfirmacion>
  );
}
