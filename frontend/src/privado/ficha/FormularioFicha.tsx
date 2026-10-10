import { useState, type FormEvent } from 'react';
import { ErrorApi, mensajeDe } from '../../compartido/api/errores';
import type { ActualizarFichaDto, FichaJugadorDto, GrupoSanguineo } from '../../compartido/api/tiposFicha';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { Campo } from '../../compartido/componentes/Campo';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { OPCIONES_DE_GRUPO_SANGUINEO } from './textos';

type Campos = Record<keyof ActualizarFichaDto, string>;

interface Props {
  ficha: FichaJugadorDto;
  /** Envía los datos a la API y devuelve la ficha ya guardada; si falla, debe lanzar el error. */
  alGuardar: (datos: ActualizarFichaDto) => Promise<FichaJugadorDto>;
}

function camposDe(ficha: FichaJugadorDto): Campos {
  return {
    celular: ficha.contacto.celular ?? '',
    nombreResponsable: ficha.contacto.nombreResponsable ?? '',
    emergenciaNombre: ficha.contactoEmergencia.nombre ?? '',
    emergenciaParentesco: ficha.contactoEmergencia.parentesco ?? '',
    emergenciaCelular: ficha.contactoEmergencia.celular ?? '',
    entidadSalud: ficha.seguridadSocial.entidadSalud ?? '',
    lugarAtencion: ficha.seguridadSocial.lugarAtencion ?? '',
    grupoSanguineo: ficha.datosClinicos?.grupoSanguineo ?? '',
    alergias: ficha.datosClinicos?.alergias ?? '',
    enfermedades: ficha.datosClinicos?.enfermedades ?? '',
    medicamentos: ficha.datosClinicos?.medicamentos ?? '',
    observaciones: ficha.datosClinicos?.observaciones ?? '',
  };
}

/**
 * Formulario de la ficha para quien puede cambiarla: contacto, contacto de emergencia, seguridad
 * social y datos clínicos, con un solo botón "Guardar". Guardar reemplaza todos esos datos. Solo
 * el celular es obligatorio y, si el jugador es menor de edad, el responsable; quien decide es la
 * API, y sus errores se pintan junto a cada campo. El correo se muestra, pero no se cambia aquí.
 */
export function FormularioFicha({ ficha, alGuardar }: Props) {
  const [datos, setDatos] = useState<Campos>(() => camposDe(ficha));
  const [errores, setErrores] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [guardado, setGuardado] = useState(false);
  const [enviando, setEnviando] = useState(false);

  const campo = (nombre: keyof Campos) => ({
    valor: datos[nombre],
    error: errores[nombre]?.[0],
    alCambiar: (valor: string) => {
      setGuardado(false);
      setDatos((previos) => ({ ...previos, [nombre]: valor }));
    },
  });

  async function enviar(evento: FormEvent) {
    evento.preventDefault();
    setError(null);
    setErrores({});
    setGuardado(false);
    setEnviando(true);

    try {
      const guardada = await alGuardar({
        celular: datos.celular,
        nombreResponsable: datos.nombreResponsable || null,
        emergenciaNombre: datos.emergenciaNombre || null,
        emergenciaParentesco: datos.emergenciaParentesco || null,
        emergenciaCelular: datos.emergenciaCelular || null,
        entidadSalud: datos.entidadSalud || null,
        lugarAtencion: datos.lugarAtencion || null,
        grupoSanguineo: (datos.grupoSanguineo || null) as GrupoSanguineo | null,
        alergias: datos.alergias || null,
        enfermedades: datos.enfermedades || null,
        medicamentos: datos.medicamentos || null,
        observaciones: datos.observaciones || null,
      });
      // Se muestra lo que quedó guardado, que la API devuelve ya sin espacios sobrantes.
      setDatos(camposDe(guardada));
      setGuardado(true);
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.codigo === 'datos_invalidos') {
        setErrores(fallo.errores);
        setError('Revisa los datos marcados. No se guardó nada.');
      } else {
        setError(mensajeDe(fallo));
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="columna" onSubmit={enviar} noValidate>
      <Tarjeta titulo="Contacto">
        <p>
          <span className="texto-suave">Correo: </span>
          {ficha.contacto.correo}
        </p>
        <p className="texto-suave">
          El correo no se cambia desde la ficha. El celular y el responsable son de la cuenta: valen para todos sus
          jugadores y clubes.
        </p>
        <div className="rejilla">
          <Campo etiqueta="Celular" type="tel" maxLength={20} required {...campo('celular')} />
          <Campo
            etiqueta="Nombre del padre, madre o responsable"
            maxLength={160}
            required={ficha.esMenorDeEdad}
            ayuda={ficha.esMenorDeEdad ? 'Obligatorio: el jugador es menor de 18 años.' : 'Opcional.'}
            {...campo('nombreResponsable')}
          />
        </div>
      </Tarjeta>

      <Tarjeta titulo="Contacto de emergencia">
        <p className="texto-suave">A quién llamar si le pasa algo al jugador. Es opcional.</p>
        <div className="rejilla">
          <Campo etiqueta="Nombre" maxLength={160} {...campo('emergenciaNombre')} />
          <Campo etiqueta="Parentesco" maxLength={40} {...campo('emergenciaParentesco')} />
          <Campo etiqueta="Celular" type="tel" maxLength={20} {...campo('emergenciaCelular')} />
        </div>
      </Tarjeta>

      <Tarjeta titulo="Seguridad social">
        <div className="rejilla">
          <Campo etiqueta="Entidad de salud" maxLength={120} {...campo('entidadSalud')} />
          <Campo etiqueta="Dónde lo atienden" maxLength={200} {...campo('lugarAtencion')} />
        </div>
      </Tarjeta>

      <Tarjeta titulo="Datos clínicos">
        <p className="texto-suave">
          Opcionales. Los ven la familia, el entrenador de la categoría y el presidente del club.
        </p>
        <Campo etiqueta="Grupo sanguíneo" opciones={OPCIONES_DE_GRUPO_SANGUINEO} {...campo('grupoSanguineo')} />
        <Campo etiqueta="Alergias" lineas={3} maxLength={1000} {...campo('alergias')} />
        <Campo etiqueta="Enfermedades o condiciones" lineas={3} maxLength={1000} {...campo('enfermedades')} />
        <Campo etiqueta="Medicamentos" lineas={3} maxLength={1000} {...campo('medicamentos')} />
        <Campo etiqueta="Observaciones" lineas={3} maxLength={1000} {...campo('observaciones')} />
      </Tarjeta>

      {error && <Aviso tono="error">{error}</Aviso>}
      {guardado && <Aviso tono="exito">Ficha guardada.</Aviso>}
      <div className="fila">
        <Boton type="submit" cargando={enviando} textoCargando="Guardando…">
          Guardar
        </Boton>
      </div>
    </form>
  );
}
