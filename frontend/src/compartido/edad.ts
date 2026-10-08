const EDAD_ADULTA = 18;

/**
 * Indica si quien nació en esa fecha (`AAAA-MM-DD`) tiene hoy menos de 18 años cumplidos. Solo
 * sirve para avisar en pantalla de que el responsable es obligatorio: quien decide es la API.
 * Con una fecha vacía o incompleta devuelve `false`.
 */
export function esMenorDeEdad(fechaNacimiento: string, hoy: Date = new Date()): boolean {
  const partes = /^(\d{4})-(\d{2})-(\d{2})$/.exec(fechaNacimiento);
  if (!partes) {
    return false;
  }

  const [anio, mes, dia] = [Number(partes[1]), Number(partes[2]), Number(partes[3])];
  const mesDeHoy = hoy.getMonth() + 1;
  const aunNoCumple = mesDeHoy < mes || (mesDeHoy === mes && hoy.getDate() < dia);
  const anios = hoy.getFullYear() - anio - (aunNoCumple ? 1 : 0);

  return anios < EDAD_ADULTA;
}
