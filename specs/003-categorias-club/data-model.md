# Modelo de datos: Categorías del club

**Funcionalidad**: `003-categorias-club` | **Fecha**: 2026-10-08

Esta funcionalidad crea cinco tablas y amplía una entidad de la 001 y la 002
([modelo de la 001](../001-base-multiclub/data-model.md),
[modelo de la 002](../002-ingreso-club/data-model.md)). Aquí solo se describe lo nuevo y lo que
cambia. Los motivos de cada decisión están en [research.md](research.md).

## Vista general

```text
Club 1 ──── N Categoria 1 ──── N Equipo
                 │  │              │   │
                 │  │              │   └── N JugadorEquipo N ──── 1 UsuarioRol (jugador)
                 │  │              │
                 │  │              └────── N EntrenadorEquipo N ──┐
                 │  │                                             │
                 │  └── N AsignacionEntrenadorCategoria 1 ────────┘
                 │              N
                 │              └──── 1 UsuarioRol (ENTRENADOR, DIRECTIVO o PRESIDENTE)
                 │
                 └── 0..1 ◀── UsuarioRol.CategoriaId (jugador)
```

Todas las entidades nuevas implementan `IPerteneceAClub`: llevan `ClubId`, quedan bajo el filtro
global de aislamiento (§7.1) y se borran en cascada al eliminar el club. Una sola migración de
Entity Framework crea las tablas y las columnas.

## Categoria (nueva)

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| ClubId | Guid | Obligatorio. Foránea a `Club`, borrado en cascada |
| Anio | entero | Obligatorio. Año de cuatro cifras no posterior al año en curso (RF-002). No se puede cambiar (RF-004). Es el nombre de la categoría (RF-001) |
| Activa | booleano | Obligatorio. Nace en verdadero |
| Usada | booleano | Obligatorio. Nace en falso; pasa a verdadero la primera vez que entra un jugador o un entrenador y no vuelve a falso |
| CreadaEn | fecha y hora | UTC |

**Índice único**: `(ClubId, Anio)`. No hay más de una categoría por año en un club, contando las
inactivas (RF-003).

**Reglas**:

- No lleva nombre libre, horario, sede ni cupo (supuestos de la spec).
- Solo se borra físicamente con `Usada` en falso; sus equipos se borran con ella (RF-004a).
- No se desactiva mientras algún integrante tenga su `CategoriaId` (RF-005).
- Inactiva, no admite jugadores, entrenadores ni equipos nuevos (RF-007, RF-022).

### Transiciones de `Activa`

```text
  crear ──▶ ACTIVA ───── desactivar (sin jugadores) ─────▶ INACTIVA
               ▲                                              │
               └──────────────── reactivar ───────────────────┘

  ACTIVA o INACTIVA, con Usada = falso ── borrar ──▶ (fila borrada, con sus equipos)
```

| Operación | Efectos en la misma transacción |
| --- | --- |
| Crear | Recoge a los jugadores sin categoría nacidos ese año; si entra alguno, `Usada = true` (RF-010) |
| Desactivar | Desactiva sus asignaciones y borra los `EntrenadorEquipo` de ellas (RF-006) |
| Reactivar | Queda sin entrenadores; recoge a los jugadores sin categoría nacidos ese año (RF-010) |

## Equipo (nueva)

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| ClubId | Guid | Obligatorio. Foránea a `Club`, borrado en cascada |
| CategoriaId | Guid | Obligatorio. Foránea a `Categoria`, borrado en cascada. No se puede cambiar: un equipo no pasa a otra categoría (RF-024) |
| Nombre | texto (30) | Obligatorio, sin espacios sobrantes, 30 caracteres como máximo (RF-023) |
| NombreNormalizado | texto (30) | `Nombre` en minúsculas. Solo para la unicidad |
| Activo | booleano | Obligatorio. Nace en verdadero. No existe la reactivación |
| Usado | booleano | Obligatorio. Nace en falso; pasa a verdadero la primera vez que tiene un jugador o un entrenador |
| CreadoEn | fecha y hora | UTC |

**Índice único parcial**: `(CategoriaId, NombreNormalizado)` sobre las filas con `Activo`
verdadero. El nombre no se repite dentro de la categoría sin distinguir mayúsculas; categorías
distintas pueden repetirlo (RF-023).

**Reglas**:

- Solo se crea en una categoría activa (RF-022).
- Cambiar el nombre conserva sus jugadores y entrenadores (escenario 5.9).
- Desactivar borra sus `JugadorEquipo` y sus `EntrenadorEquipo`; los jugadores siguen en la
  categoría y los entrenadores siguen asignados a ella (RF-030).
- Solo se borra físicamente con `Usado` en falso (RF-024a).
- Un equipo inactivo no aparece en ninguna respuesta de la API.

## AsignacionEntrenadorCategoria (nueva)

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| ClubId | Guid | Obligatorio. Foránea a `Club`, borrado en cascada |
| CategoriaId | Guid | Obligatorio. Foránea a `Categoria`, borrado en cascada |
| UsuarioRolId | Guid | Obligatorio. Foránea a `UsuarioRol`, borrado en cascada |
| Activa | booleano | Obligatorio |
| CreadaEn | fecha y hora | UTC |

**Índice único**: `(CategoriaId, UsuarioRolId)`. **Índice**: `(UsuarioRolId, Activa)`, para "las
categorías de este entrenador".

**Reglas**:

- El integrante debe ser del mismo club, estar `APROBADO` y tener rol `ENTRENADOR`, `DIRECTIVO` o
  `PRESIDENTE`, y la categoría debe estar activa (RF-007, RF-017).
- Asignar crea la fila o la vuelve a activar; asignar dos veces no tiene efecto (RF-020).
- Retirar pone `Activa = false` y borra sus `EntrenadorEquipo`. La fila no se borra (RF-021).
- No cambia `UsuarioRol.Rol`: el integrante conserva su único rol y su alcance (RF-017a).
- Es lo que determina qué categorías ve un integrante con rol `ENTRENADOR` (RF-034, §12.3).

## EntrenadorEquipo (nueva)

Qué equipos de la categoría dirige una asignación.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| AsignacionEntrenadorCategoriaId | Guid | Clave primaria compuesta. Foránea, borrado en cascada |
| EquipoId | Guid | Clave primaria compuesta. Foránea a `Equipo`, borrado en cascada |
| ClubId | Guid | Obligatorio. Foránea a `Club`, borrado en cascada |

**Reglas**:

- La asignación debe estar activa y el equipo debe ser activo y de la misma categoría (RF-028).
- Puede no haber ninguna fila: el entrenador lo es de la categoría en general (escenario 5.8).
- No cambia lo que ve el entrenador: ve a todos los jugadores de la categoría (RF-029).
- Las filas se borran al quitar el equipo de la lista, al retirar la asignación y al desactivar el
  equipo o la categoría. No se guarda historial.

## JugadorEquipo (nueva)

En qué equipos juega un jugador.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| UsuarioRolId | Guid | Clave primaria compuesta. Foránea a `UsuarioRol`, borrado en cascada |
| EquipoId | Guid | Clave primaria compuesta. Foránea a `Equipo`, borrado en cascada |
| ClubId | Guid | Obligatorio. Foránea a `Club`, borrado en cascada |

**Reglas**:

- El equipo debe ser activo y de la categoría actual del jugador (RF-025).
- Un jugador puede tener varias filas o ninguna (RF-026). La ubicación automática nunca crea una.
- Las filas del jugador se borran cuando cambia de categoría, se retira o deja de ser JUGADOR
  (RF-027, RF-042). No se guarda historial (§11.3).

## UsuarioRol (cambia)

| Campo nuevo | Tipo | Reglas |
| --- | --- | --- |
| CategoriaId | Guid | Opcional. Foránea a `Categoria`, sin cascada. Categoría actual del jugador (RF-013) |
| Activo | booleano | Obligatorio. Verdadero por defecto, también para las filas que ya existen. Falso significa retirado del club (§14.1) |
| RetiradoEn | fecha y hora | Opcional. UTC. Solo tiene valor mientras está retirado |
| RetiradoPorUsuarioId | Guid | Opcional. Foránea a `Usuario`; pasa a nulo si esa cuenta se elimina |
| RetiradoPorNombre | texto (161) | Opcional. Nombres y apellidos de quien lo retiró, copiados en ese momento (§13) |

**Índice nuevo**: `(ClubId, CategoriaId)`, para los jugadores de una categoría y el recuento.

**Reglas**:

- `CategoriaId` solo puede tener valor si `Rol = JUGADOR`, `EstadoIngreso = APROBADO` y
  `Activo` es verdadero (RF-012). La categoría debe ser del mismo club y estar activa (RF-007).
- `Activo` solo puede ser falso si `Rol = JUGADOR` y `EstadoIngreso = APROBADO` (RF-041).
- Los tres campos del retiro se rellenan juntos al retirar y se vacían juntos al reincorporar.
- Un integrante retirado no pasa la autorización de ningún endpoint del club (RF-043). Su fila se
  conserva, así que su documento sigue ocupado en el club y su correo sigue siendo el de un
  integrante de ese club (RF-046).
- El retiro es de cada pertenencia: la misma cuenta puede estar retirada en un club y activa en
  otro.

### Conjuntos derivados (no se guardan)

```text
Jugador del club   = Rol JUGADOR  y  EstadoIngreso APROBADO  y  Activo
Sin categoría      = jugador del club con CategoriaId nulo
Retirados          = Rol JUGADOR  y  Activo falso
Fuera de su año    = jugador cuyo año de nacimiento ≠ Categoria.Anio
Número de jugadores de una categoría = jugadores del club con ese CategoriaId
```

Un jugador que está en dos equipos cuenta una sola vez en el total de su categoría.

### Transiciones de la categoría de un jugador

```text
  aprobar como JUGADOR ──▶ categoría activa de su año, o SIN CATEGORÍA
  reincorporar         ──▶ categoría activa de su año, o SIN CATEGORÍA

  SIN CATEGORÍA ── crear o reactivar la categoría de su año ──▶ EN CATEGORÍA   (automática)
  SIN CATEGORÍA ── el PRESIDENTE lo ubica ──────────────────▶ EN CATEGORÍA
  EN CATEGORÍA  ── el PRESIDENTE lo pasa a otra ────────────▶ EN OTRA CATEGORÍA  (sale de sus equipos)
  EN CATEGORÍA o SIN CATEGORÍA ── retirar ──────────────────▶ RETIRADO  (sin categoría ni equipos)
```

- La ubicación automática solo actúa sobre quien no tiene categoría (RF-011).
- No existe "dejar sin categoría" a quien ya tiene una (supuesto de la spec).
- Toda transición se ejecuta con la fila del club bloqueada (research §4).

### Estado de un integrante frente al club

```text
EstadoIngreso EN_ESPERA                 → solo la sala de espera            (002)
EstadoIngreso APROBADO y Activo         → entra según su rol
EstadoIngreso APROBADO y Activo falso   → solo el aviso "ya no estás en este club"   (nuevo)
```

El retiro no es un estado de ingreso (§14.1).

## Quién puede qué

| Operación | PRESIDENTE | DIRECTIVO | ENTRENADOR | JUGADOR |
| --- | --- | --- | --- | --- |
| Crear, desactivar, reactivar o borrar una categoría | Sí | No | No | No |
| Asignar o retirar entrenadores; indicar qué equipos dirigen | Sí | No | No | No |
| Crear, renombrar, desactivar o borrar equipos | Sí | No | No | No |
| Ubicar o cambiar de categoría; poner o sacar de un equipo | Sí | No | No | No |
| Retirar o reincorporar a un jugador | Sí | No | No | No |
| Ver todas las categorías, "Sin categoría" y "Retirados" | Sí | Sí | No | No |
| Ver una categoría con sus equipos y jugadores | Todas | Todas | Solo las activas que tiene asignadas | No |
| Ver su categoría, sus equipos y el nombre de sus entrenadores | No aplica | No aplica | No aplica | Sí, solo lo suyo |

Estar asignado como entrenador no cambia la fila de un PRESIDENTE ni la de un DIRECTIVO en esta
tabla (RF-017a). El DESARROLLADOR no puede nada de esto (RF-038).

## Lo que no se guarda

- **Historial** de las categorías o de los equipos por los que pasó un jugador (§11.2, §11.3).
- **Un retiro anterior**: al reincorporar se vacían los datos del retiro.
- **Nombre libre, horario, sede o cupo** de la categoría.
- **La mensualidad**: llega con su funcionalidad y no depende de la categoría ni del equipo.
- **La ficha del jugador**: llega con la funcionalidad de jugadores, que decidirá si
  `CategoriaId` se queda en el integrante o pasa a la ficha (research §1).
