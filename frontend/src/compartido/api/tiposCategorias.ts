// Tipos de los esquemas de specs/003-categorias-club/contracts/api.yaml, con los mismos nombres.
// Van aparte de tipos.ts para que ningún archivo supere las 250 líneas.

/** Roles de quien puede quedar asignado a una categoría. */
export type RolDeEntrenador = 'ENTRENADOR' | 'DIRECTIVO' | 'PRESIDENTE';

export interface CrearCategoriaDto {
  anio: number;
}

export interface NombreEquipoDto {
  nombre: string;
}

export interface EquiposDeEntrenadorDto {
  /** Equipos activos de la categoría que dirige; puede ir vacía. */
  equipoIds: string[];
}

export interface UbicarJugadorDto {
  categoriaId: string;
}

export interface EquipoDto {
  equipoId: string;
  nombre: string;
  numeroJugadores: number;
  /** Verdadero si nunca ha tenido jugadores ni entrenadores. */
  sePuedeBorrar: boolean;
}

export interface EquipoDeReferenciaDto {
  equipoId: string;
  nombre: string;
}

export interface EntrenadorDeCategoriaDto {
  usuarioRolId: string;
  nombres: string;
  apellidos: string;
  rol: RolDeEntrenador;
  /** Equipos que dirige; vacía si es entrenador de la categoría en general. */
  equipos: EquipoDeReferenciaDto[];
}

export interface CandidatoEntrenadorDto {
  usuarioRolId: string;
  nombres: string;
  apellidos: string;
  rol: RolDeEntrenador;
}

export interface JugadorDeCategoriaDto {
  usuarioRolId: string;
  nombres: string;
  apellidos: string;
  anioNacimiento: number;
  /** Verdadero si su categoría no es la de su año de nacimiento; falso en "Sin categoría". */
  fueraDeSuAnio: boolean;
  equipos: EquipoDeReferenciaDto[];
}

export interface CategoriaDto {
  categoriaId: string;
  anio: number;
  activa: boolean;
  /** Verdadero si nunca ha tenido jugadores ni entrenadores. */
  sePuedeBorrar: boolean;
  /** Cada jugador cuenta una vez, aunque esté en varios equipos. */
  numeroJugadores: number;
  equipos: EquipoDto[];
  entrenadores: EntrenadorDeCategoriaDto[];
}

export interface CategoriaDetalleDto extends CategoriaDto {
  jugadores: JugadorDeCategoriaDto[];
}

export interface CategoriaConUbicadosDto {
  categoria: CategoriaDto;
  /** Cuántos jugadores sin categoría nacidos ese año entraron; puede ser 0. */
  jugadoresUbicados: number;
}

export interface JugadorRetiradoDto {
  usuarioRolId: string;
  nombres: string;
  apellidos: string;
  anioNacimiento: number;
  /** Nombre de quien lo retiró, tal como era en ese momento. */
  retiradoPor: string;
  retiradoEn: string;
}

export interface ReincorporacionDto {
  usuarioRolId: string;
  /** Categoría en la que quedó; nulo si quedó sin categoría. */
  categoriaId: string | null;
  anio: number | null;
}

/** Lo único que una familia ve de un entrenador: sin identificador, rol ni contacto. */
export interface EntrenadorParaFamiliaDto {
  nombres: string;
  apellidos: string;
  /** Nombres de los equipos que dirige; vacía si lo es de la categoría en general. */
  equipos: string[];
}

export interface MiCategoriaDto {
  /** Nulo si el jugador todavía no tiene categoría. */
  categoria: {
    anio: number;
    equipos: string[];
    entrenadores: EntrenadorParaFamiliaDto[];
  } | null;
}
