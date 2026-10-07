import { useState, type FormEvent } from 'react';
import { ErrorApi, mensajeDe } from '../api/errores';
import type { ActualizarConfiguracionClubDto } from '../api/tipos';
import { Aviso } from './Aviso';
import { Boton } from './Boton';
import { Campo } from './Campo';

type Campos = Record<keyof ActualizarConfiguracionClubDto, string>;

interface Props {
  /** Datos actuales del club. */
  inicial: ActualizarConfiguracionClubDto;
  /** Envía los datos a la API; si falla, debe lanzar el error. */
  alGuardar: (datos: ActualizarConfiguracionClubDto) => Promise<void>;
}

/**
 * Formulario de los datos de un club: nombre, sede, dirección y contacto. Lo usan el presidente,
 * sobre su club, y el desarrollador, desde su panel. No tiene escudo ni colores.
 */
export function FormularioDatosClub({ inicial, alGuardar }: Props) {
  const [datos, setDatos] = useState<Campos>({
    nombre: inicial.nombre,
    sede: inicial.sede ?? '',
    direccion: inicial.direccion ?? '',
    correoContacto: inicial.correoContacto ?? '',
    telefonoContacto: inicial.telefonoContacto ?? '',
  });
  const [errores, setErrores] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [guardado, setGuardado] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const cambiar = (campo: keyof Campos) => (valor: string) => {
    setGuardado(false);
    setDatos((previos) => ({ ...previos, [campo]: valor }));
  };

  async function enviar(evento: FormEvent) {
    evento.preventDefault();
    setError(null);
    setErrores({});
    setGuardado(false);
    setEnviando(true);
    try {
      await alGuardar({
        nombre: datos.nombre,
        sede: datos.sede || null,
        direccion: datos.direccion || null,
        correoContacto: datos.correoContacto || null,
        telefonoContacto: datos.telefonoContacto || null,
      });
      setGuardado(true);
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.codigo === 'datos_invalidos') {
        setErrores(fallo.errores);
      } else if (fallo instanceof ErrorApi && fallo.codigo === 'nombre_de_club_repetido') {
        setErrores({ nombre: [fallo.title] });
      } else {
        setError(mensajeDe(fallo));
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="columna" onSubmit={enviar} noValidate>
      {error && <Aviso tono="error">{error}</Aviso>}
      {guardado && <Aviso tono="exito">Datos guardados.</Aviso>}
      <Campo
        etiqueta="Nombre del club"
        valor={datos.nombre}
        alCambiar={cambiar('nombre')}
        error={errores.nombre?.[0]}
        maxLength={120}
        required
      />
      <div className="rejilla">
        <Campo
          etiqueta="Sede"
          valor={datos.sede}
          alCambiar={cambiar('sede')}
          error={errores.sede?.[0]}
          maxLength={120}
        />
        <Campo
          etiqueta="Dirección"
          valor={datos.direccion}
          alCambiar={cambiar('direccion')}
          error={errores.direccion?.[0]}
          maxLength={200}
        />
      </div>
      <div className="rejilla">
        <Campo
          etiqueta="Correo de contacto"
          type="email"
          valor={datos.correoContacto}
          alCambiar={cambiar('correoContacto')}
          error={errores.correoContacto?.[0]}
        />
        <Campo
          etiqueta="Teléfono de contacto"
          type="tel"
          valor={datos.telefonoContacto}
          alCambiar={cambiar('telefonoContacto')}
          error={errores.telefonoContacto?.[0]}
          maxLength={20}
        />
      </div>
      <div className="fila">
        <Boton type="submit" cargando={enviando} textoCargando="Guardando…">
          Guardar datos
        </Boton>
      </div>
    </form>
  );
}
