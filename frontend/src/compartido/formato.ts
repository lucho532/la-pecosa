import type { Rol } from './api/tipos';

const FECHA = new Intl.DateTimeFormat('es-CO', { day: 'numeric', month: 'long', year: 'numeric' });
const FECHA_Y_HORA = new Intl.DateTimeFormat('es-CO', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  hour: 'numeric',
  minute: '2-digit',
});

/** Fecha en español a partir de la fecha en formato ISO que entrega la API. */
export function fecha(iso: string): string {
  return FECHA.format(new Date(iso));
}

/** Fecha y hora en español a partir de la fecha en formato ISO que entrega la API. */
export function fechaYHora(iso: string): string {
  return FECHA_Y_HORA.format(new Date(iso));
}

const ROLES: Record<Rol, string> = {
  DESARROLLADOR: 'Desarrollador',
  PRESIDENTE: 'Presidente',
  DIRECTIVO: 'Directivo',
  ENTRENADOR: 'Entrenador',
  JUGADOR: 'Jugador',
};

/** Nombre de un rol para mostrar. */
export function nombreDeRol(rol: Rol): string {
  return ROLES[rol];
}
