import { useState } from 'react';
import { api } from '../../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { CorregirIdentidadDto, FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { Campo } from '../../compartido/componentes/Campo';
import { DialogoConfirmacion } from '../../compartido/componentes/DialogoConfirmacion';

interface Props {
  ficha: FichaJugadorDto;
  /** Dirección de la ficha en la API. */
  rutaDeLaFicha: string;
  /** Recibe la ficha tal como la devolvió la API con la identidad corregida. */
  alGuardar: (ficha: FichaJugadorDto) => void;
  alCancelar: () => void;
}

/**
 * Diálogo con el que el presidente corrige los nombres, los apellidos y la fecha de nacimiento de
 * un jugador cuando se registraron mal. La fecha no puede ser futura. Corregirla no cambia de
 * categoría a quien ya tiene una; a quien no tiene, la API lo ubica en la del año nuevo si existe.
 */
export function DialogoCorregirIdentidad({ ficha, rutaDeLaFicha, alGuardar, alCancelar }: Props) {
  const [datos, setDatos] = useState<CorregirIdentidadDto>({
    nombres: ficha.nombres,
    apellidos: ficha.apellidos,
    fechaNacimiento: ficha.fechaNacimiento,
  });
  const [errores, setErrores] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string>();
  const [enviando, setEnviando] = useState(false);
  // El día de hoy en UTC, que es con el que compara la API.
  const hoy = new Date().toISOString().slice(0, 10);

  const campo = (nombre: keyof CorregirIdentidadDto) => ({
    valor: datos[nombre],
    error: errores[nombre]?.[0],
    alCambiar: (valor: string) => setDatos((previos) => ({ ...previos, [nombre]: valor })),
  });

  async function guardar() {
    setError(undefined);
    setErrores({});
    setEnviando(true);
    try {
      alGuardar(await api.put<FichaJugadorDto>(`${rutaDeLaFicha}/identidad`, datos));
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
      titulo="Corregir la identidad"
      textoConfirmar="Guardar corrección"
      deshabilitado={datos.fechaNacimiento === ''}
      cargando={enviando}
      error={error}
      alConfirmar={() => void guardar()}
      alCancelar={alCancelar}
    >
      <p className="texto-suave">
        {ficha.categoriaAnio !== null
          ? `Si cambias el año de nacimiento, sigue en la categoría ${ficha.categoriaAnio}.`
          : ficha.retirado
            ? 'Está retirado: corregir la fecha no lo ubica en ninguna categoría.'
            : 'No tiene categoría: si existe la de su año de nacimiento, entra en ella al guardar.'}
      </p>
      <Campo etiqueta="Nombres" maxLength={80} required {...campo('nombres')} />
      <Campo etiqueta="Apellidos" maxLength={80} required {...campo('apellidos')} />
      <Campo etiqueta="Fecha de nacimiento" type="date" max={hoy} required {...campo('fechaNacimiento')} />
    </DialogoConfirmacion>
  );
}
