import type { FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { GrupoDeDatos } from './GrupoDeDatos';
import { nombreDeGrupoSanguineo } from './textos';

/**
 * Contacto, contacto de emergencia, seguridad social y datos clínicos de la ficha como texto, para
 * quien la consulta sin poder cambiarla. Los datos clínicos solo se pintan si la API los entregó:
 * a un directivo no le llegan, así que para él ese apartado no existe.
 */
export function FichaDeSoloLectura({ ficha }: { ficha: FichaJugadorDto }) {
  const clinicos = ficha.datosClinicos;

  return (
    <>
      <GrupoDeDatos
        titulo="Contacto"
        datos={[
          { etiqueta: 'Correo', valor: ficha.contacto.correo },
          { etiqueta: 'Celular', valor: ficha.contacto.celular },
          { etiqueta: 'Padre, madre o responsable', valor: ficha.contacto.nombreResponsable },
        ]}
      />
      <GrupoDeDatos
        titulo="Contacto de emergencia"
        datos={[
          { etiqueta: 'Nombre', valor: ficha.contactoEmergencia.nombre },
          { etiqueta: 'Parentesco', valor: ficha.contactoEmergencia.parentesco },
          { etiqueta: 'Celular', valor: ficha.contactoEmergencia.celular },
        ]}
      />
      <GrupoDeDatos
        titulo="Seguridad social"
        datos={[
          { etiqueta: 'Entidad de salud', valor: ficha.seguridadSocial.entidadSalud },
          { etiqueta: 'Dónde lo atienden', valor: ficha.seguridadSocial.lugarAtencion },
        ]}
      />
      {clinicos && (
        <GrupoDeDatos
          titulo="Datos clínicos"
          datos={[
            {
              etiqueta: 'Grupo sanguíneo',
              valor: clinicos.grupoSanguineo ? nombreDeGrupoSanguineo(clinicos.grupoSanguineo) : null,
            },
            { etiqueta: 'Alergias', valor: clinicos.alergias, largo: true },
            { etiqueta: 'Enfermedades o condiciones', valor: clinicos.enfermedades, largo: true },
            { etiqueta: 'Medicamentos', valor: clinicos.medicamentos, largo: true },
            { etiqueta: 'Observaciones', valor: clinicos.observaciones, largo: true },
          ]}
        />
      )}
    </>
  );
}
