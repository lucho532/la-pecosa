# Lista de verificación de calidad de la especificación: Categorías del club

**Propósito**: validar que la especificación está completa y tiene la calidad necesaria antes de pasar a la planificación
**Creada**: 2026-10-07
**Funcionalidad**: [spec.md](../spec.md)

## Calidad del contenido

- [x] Sin detalles de implementación (lenguajes, frameworks, APIs)
- [x] Centrada en el valor para el usuario y en las necesidades del negocio
- [x] Escrita para personas no técnicas
- [x] Todas las secciones obligatorias completas

## Completitud de los requisitos

- [x] No quedan marcadores [NEEDS CLARIFICATION]
- [x] Los requisitos son comprobables y no ambiguos
- [x] Los criterios de éxito son medibles
- [x] Los criterios de éxito no dependen de la tecnología
- [x] Todos los escenarios de aceptación están definidos
- [x] Los casos límite están identificados
- [x] El alcance está claramente delimitado
- [x] Las dependencias y los supuestos están identificados

## Preparación de la funcionalidad

- [x] Todos los requisitos funcionales tienen criterios de aceptación claros
- [x] Los escenarios de usuario cubren los flujos principales
- [x] La funcionalidad cumple los resultados medibles definidos en los criterios de éxito
- [x] No se filtran detalles de implementación en la especificación

## Notas

- Las dos aclaraciones se resolvieron con el propietario el 2026-10-07: el DIRECTIVO solo
  consulta las categorías, y un jugador aprobado cuya categoría de año no existe queda en "Sin
  categoría" hasta que se crea.
- La descripción de entrada dice "el PRESIDENTE o DIRECTIVO"; la spec sigue la respuesta del
  propietario y la constitución (§8 y §11.2), no la descripción literal.
- Segunda ronda con el propietario el 2026-10-07: las familias ven quién entrena a su jugador; un
  PRESIDENTE puede quedar asignado como entrenador sin cambiar de rol; una categoría puede
  dividirse en equipos; el entrenador se asigna a la categoría y ve todos sus equipos; un jugador
  puede estar en varios equipos a la vez. La lista se volvió a validar después de estos cambios.
- La spec decide cuatro cosas que la constitución todavía no recoge y que falta reflejar en ella:
  qué pasa cuando no existe la categoría del año del jugador (§12.1.1); los equipos dentro de una
  categoría (§2.2 y §11); que un PRESIDENTE puede estar asignado como entrenador de una categoría
  (§8 y §12.3); y que la familia ve el nombre de los entrenadores de su jugador (§8).
- Decisiones tomadas por defecto, sin preguntar, que conviene revisar en `/speckit-clarify`: una
  categoría con jugadores no se puede desactivar; al desactivarla se retiran sus entrenadores; el
  PRESIDENTE puede ubicar a un jugador en una categoría que no es la de su año; las listas de
  jugadores solo muestran nombre, apellidos, año de nacimiento y equipos; la familia ve nombre y
  apellidos de los entrenadores, sin datos de contacto; estar en un equipo es opcional; un equipo
  se puede desactivar aunque tenga jugadores; el nombre de un equipo tiene 30 caracteres como
  máximo.
