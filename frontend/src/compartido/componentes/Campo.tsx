import { useId, type InputHTMLAttributes } from 'react';

interface Opcion {
  valor: string;
  texto: string;
}

interface Props extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
  etiqueta: string;
  valor: string;
  alCambiar?: (valor: string) => void;
  /** Mensaje de error del campo, tal como lo devuelve la API. */
  error?: string;
  ayuda?: string;
  /** Si se indican, el campo es una lista desplegable. */
  opciones?: Opcion[];
  /** Si se indica, el campo es un área de texto con ese número de líneas a la vista. */
  lineas?: number;
}

/** Etiqueta, entrada y error de un campo de formulario. */
export function Campo({ etiqueta, valor, alCambiar, error, ayuda, opciones, lineas, ...resto }: Props) {
  const id = useId();
  const idMensaje = `${id}-mensaje`;
  const comunes = {
    id,
    value: valor,
    'aria-invalid': error ? true : undefined,
    'aria-describedby': error || ayuda ? idMensaje : undefined,
  };

  return (
    <div className="campo">
      <label htmlFor={id}>{etiqueta}</label>
      {opciones ? (
        <select
          {...comunes}
          required={resto.required}
          disabled={resto.disabled}
          onChange={(evento) => alCambiar?.(evento.target.value)}
        >
          {opciones.map((opcion) => (
            <option key={opcion.valor} value={opcion.valor}>
              {opcion.texto}
            </option>
          ))}
        </select>
      ) : lineas ? (
        <textarea
          {...comunes}
          rows={lineas}
          maxLength={resto.maxLength}
          required={resto.required}
          disabled={resto.disabled}
          onChange={(evento) => alCambiar?.(evento.target.value)}
        />
      ) : (
        <input {...resto} {...comunes} onChange={(evento) => alCambiar?.(evento.target.value)} />
      )}
      {error ? (
        <span id={idMensaje} className="campo-error">
          {error}
        </span>
      ) : (
        ayuda && (
          <span id={idMensaje} className="texto-suave">
            {ayuda}
          </span>
        )
      )}
    </div>
  );
}
