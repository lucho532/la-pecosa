import type { FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { EtiquetaEstado } from '../../compartido/componentes/EtiquetaEstado';
import { dia, nombreDeTipoDocumento } from '../../compartido/formato';
import { GrupoDeDatos } from './GrupoDeDatos';

interface Props {
  ficha: FichaJugadorDto;
}

/**
 * Identidad del jugador en la ficha: nombres, apellidos, documento, fecha de nacimiento, categoría
 * y equipos. Es siempre de solo lectura: lo que se puede cambiar de ella se cambia en un diálogo
 * aparte. Que no tiene categoría o que está retirado se dice con texto, no solo con color.
 */
export function SeccionIdentidad({ ficha }: Props) {
  const categoria = ficha.retirado ? (
    <EtiquetaEstado tono="neutro" texto="Retirado" />
  ) : ficha.categoriaAnio === null ? (
    <EtiquetaEstado tono="aviso" texto="Sin categoría" />
  ) : (
    `Categoría ${ficha.categoriaAnio}`
  );

  return (
    <GrupoDeDatos
      titulo="Identidad"
      datos={[
        { etiqueta: 'Nombres', valor: ficha.nombres },
        { etiqueta: 'Apellidos', valor: ficha.apellidos },
        { etiqueta: 'Tipo de documento', valor: nombreDeTipoDocumento(ficha.tipoDocumento) },
        { etiqueta: 'Número de documento', valor: ficha.numeroDocumento },
        { etiqueta: 'Fecha de nacimiento', valor: dia(ficha.fechaNacimiento) },
        { etiqueta: 'Categoría', valor: categoria },
        { etiqueta: 'Equipos', valor: ficha.equipos.length > 0 ? ficha.equipos.join(', ') : 'Sin equipo' },
      ]}
    />
  );
}
