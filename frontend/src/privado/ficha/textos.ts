import type { DocumentoPedido, GrupoSanguineo } from '../../compartido/api/tiposFicha';

/** Lo que se muestra en lugar de un dato que nadie ha escrito. */
export const SIN_REGISTRAR = 'Sin registrar';

const DOCUMENTOS: Record<DocumentoPedido, string> = {
  COPIA_DOCUMENTO_IDENTIDAD: 'Copia del documento de identidad',
  CERTIFICADO_SALUD: 'Certificado de afiliación a salud',
};

/** Nombre de un documento pedido para mostrar. */
export function nombreDeDocumentoPedido(documento: DocumentoPedido): string {
  return DOCUMENTOS[documento];
}

const GRUPOS: Record<GrupoSanguineo, string> = {
  A_POSITIVO: 'A+',
  A_NEGATIVO: 'A−',
  B_POSITIVO: 'B+',
  B_NEGATIVO: 'B−',
  AB_POSITIVO: 'AB+',
  AB_NEGATIVO: 'AB−',
  O_POSITIVO: 'O+',
  O_NEGATIVO: 'O−',
};

/** Nombre de un grupo sanguíneo para mostrar. */
export function nombreDeGrupoSanguineo(grupo: GrupoSanguineo): string {
  return GRUPOS[grupo];
}

/** Opciones del desplegable de grupo sanguíneo, con una primera opción vacía. */
export const OPCIONES_DE_GRUPO_SANGUINEO = [
  { valor: '', texto: SIN_REGISTRAR },
  ...(Object.keys(GRUPOS) as GrupoSanguineo[]).map((grupo) => ({ valor: grupo, texto: GRUPOS[grupo] })),
];
