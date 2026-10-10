import { EtiquetaEstado } from '../../compartido/componentes/EtiquetaEstado';
import { textoDeDocumentacion } from './textos';

/**
 * Estado de la documentación de un jugador en una lista: "Completa", "Falta 1" o "Faltan 2", con
 * texto además de con color (RF-032, RF-035). Si la API no entregó el dato, no pinta nada: quien
 * no puede ver los documentos tampoco sabe si faltan.
 */
export function EstadoDeDocumentacion({ pendientes }: { pendientes: number | undefined }) {
  if (pendientes === undefined) {
    return null;
  }

  return <EtiquetaEstado tono={pendientes === 0 ? 'exito' : 'aviso'} texto={textoDeDocumentacion(pendientes)} />;
}
