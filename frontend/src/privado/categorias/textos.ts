import type { Rol } from '../../compartido/api/tipos';
import type { CategoriaDto } from '../../compartido/api/tiposCategorias';

/** Roles que tienen el apartado "Categorías": quien gestiona, quien consulta todo y quien entrena. */
export function puedeVerCategorias(rol: Rol): boolean {
  return rol === 'PRESIDENTE' || rol === 'DIRECTIVO' || rol === 'ENTRENADOR';
}

/** Nombre de una persona tal como se muestra en las listas del apartado. */
export function nombreCompleto(persona: { nombres: string; apellidos: string }): string {
  return `${persona.nombres} ${persona.apellidos}`;
}

/** "1 jugador", "3 jugadores", "ningún jugador". */
export function cuantosJugadores(numero: number): string {
  if (numero === 0) {
    return 'ningún jugador';
  }

  return numero === 1 ? '1 jugador' : `${numero} jugadores`;
}

/** Lo que se le dice al presidente tras crear o reactivar una categoría (RF-010). */
export function resumenDeUbicados(categoria: CategoriaDto, ubicados: number, hecho: 'creada' | 'reactivada'): string {
  const entraron =
    ubicados === 0
      ? 'No entró ningún jugador: no hay jugadores sin categoría nacidos ese año.'
      : ubicados === 1
        ? 'Entró 1 jugador sin categoría nacido ese año.'
        : `Entraron ${ubicados} jugadores sin categoría nacidos ese año.`;

  return `Categoría ${categoria.anio} ${hecho}. ${entraron}`;
}

/** Dirección de la pantalla de la ficha de un jugador dentro de la aplicación del club. */
export function rutaDeFicha(clubId: string, usuarioRolId: string): string {
  return `/club/${clubId}/jugadores/${usuarioRolId}/ficha`;
}
