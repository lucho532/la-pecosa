# Modelo de datos: Ingreso de personas al club

**Funcionalidad**: `002-ingreso-club` | **Fecha**: 2026-10-07

Esta funcionalidad no crea ninguna tabla. Amplía tres entidades de la 001
([modelo de datos de la 001](../001-base-multiclub/data-model.md)) y añade una enumeración. Aquí
solo se describe lo que cambia; todo lo demás sigue como está.

## Vista general

```text
Usuario 1 ──── N UsuarioRol N ──── 1 Club 1 ──── N Invitacion
   ▲                │
   └── aprobado por ┘   (UsuarioRol.AprobadoPorUsuarioId, opcional)
```

Una sola migración de Entity Framework añade las columnas y el índice.

## Usuario (cambia)

| Campo nuevo | Tipo | Reglas |
| --- | --- | --- |
| NombreResponsable | texto (160) | Opcional. Nombre del padre, madre o responsable. Obligatorio al registrarse si la persona es menor de 18 años ese día (RF-010) |

Es un dato de contacto de la cuenta, como `Celular` (§12.1.2). No sale por la API salvo en la sala
de espera del club, para el PRESIDENTE y los DIRECTIVOS (RF-020).

## UsuarioRol (cambia)

| Campo | Tipo | Reglas |
| --- | --- | --- |
| EstadoIngreso | `EstadoIngreso` | Ya existía, siempre `APROBADO`. Ahora puede ser `EN_ESPERA` (RF-014) |
| AprobadoEn | fecha y hora | Nuevo. Nulo mientras está en espera y en quien entró sin sala de espera |
| AprobadoPorUsuarioId | Guid | Nuevo. Opcional. Foránea a `Usuario`; pasa a nulo si esa cuenta se elimina |
| AprobadoPorNombre | texto (161) | Nuevo. Nombres y apellidos de quien aprobó, copiados en ese momento (§13) |
| RolDeIngreso | `Rol` | Nuevo. Rol con el que quedó al aprobarse: `JUGADOR`, `ENTRENADOR` o `DIRECTIVO` (RF-025) |

**Índice nuevo**: `(ClubId, EstadoIngreso)`, para la sala de espera y la lista de aprobados.

**Reglas**:

- Los cuatro campos de aprobación se rellenan juntos, en la misma sentencia que cambia
  `EstadoIngreso` a `APROBADO`, y no se modifican después.
- Un integrante `EN_ESPERA` tiene siempre rol `JUGADOR`, no tiene categoría y no genera cobros
  (RF-017). No pasa la autorización de ningún endpoint del club (RF-016).
- El estado es de cada pertenencia: la misma cuenta puede estar `APROBADO` en un club y
  `EN_ESPERA` en otro (RF-018).
- La **sala de espera** de un club son sus integrantes `EN_ESPERA`, ordenados por `CreadoEn`
  ascendente (escenario 3.1).
- Los **ingresos aprobados** de un club son sus integrantes con `AprobadoEn` no nulo, ordenados
  por `AprobadoEn` descendente.

### Transiciones de `EstadoIngreso`

```text
  (registro o aceptación con         aprobar
   invitación del club)  ──▶ EN_ESPERA ──────────▶ APROBADO
                                 │
                                 └── rechazar ──▶ (integrante borrado)

  (invitación de presidente) ──────────────────▶ APROBADO   (sin sala de espera, 001)
```

- Aprobar y rechazar solo parten de `EN_ESPERA`; ambas son una sentencia condicionada a ese
  estado, de modo que entre dos acciones simultáneas vale la primera (RF-026).
- No existe la transición `APROBADO → EN_ESPERA`, ni se puede rechazar a un aprobado (RF-027).
- El rechazo no es un estado y no deja registro (§12.1.1).

### Quién asigna qué rol al aprobar

| Quien aprueba | `RolDeIngreso` permitido |
| --- | --- |
| PRESIDENTE | `JUGADOR`, `ENTRENADOR`, `DIRECTIVO` |
| DIRECTIVO | `JUGADOR`, `ENTRENADOR` |

`PRESIDENTE` nunca se asigna al aprobar (RF-023). Nadie aprueba su propio ingreso (RF-021). El rol
asignado reemplaza a `JUGADOR` y es el único del integrante (RF-022).

### Qué borra un rechazo

En una sola transacción (RF-027a):

1. El integrante `EN_ESPERA`.
2. Las invitaciones del club, con rol distinto de `PRESIDENTE`, enviadas al correo de su cuenta.
3. La cuenta, si se quedó sin ningún integrante. Con ella, por cascada, su `FotoPerfil` y sus
   `SolicitudRecuperacion`. La cuenta DESARROLLADOR nunca se toca.

Los integrantes de esa cuenta en otros clubes no se modifican. Es una eliminación física
justificada (§14).

## Invitacion (cambia de uso, no de columnas)

| Campo | Cambio |
| --- | --- |
| Rol | Ya no es siempre `PRESIDENTE`. Una invitación enviada desde el club lleva `JUGADOR` (RF-003) |
| AnuladaEn | Además de "reemplazada por otra", ahora también significa "cancelada por quien invita" |
| CreadaPorUsuarioId | Puede ser la cuenta de un PRESIDENTE o de un DIRECTIVO del club |

**Estado de ingreso que produce**: `PRESIDENTE` → `APROBADO`; cualquier otro rol → `EN_ESPERA`.

**Estado derivado** (`EstadoInvitacion`, no se guarda):

```text
USADA      = UsadaEn tiene valor
CANCELADA  = AnuladaEn tiene valor
VENCIDA    = ninguna de las anteriores  y  ahora >= VenceEn
PENDIENTE  = ninguna de las anteriores
```

**Reglas**:

- Las invitaciones del club (rol distinto de `PRESIDENTE`) solo las ve y gestiona el club; las de
  `PRESIDENTE`, solo el DESARROLLADOR (RF-029).
- Dentro de las del club hay como máximo una `PENDIENTE` por correo: invitar o reenviar anula las
  anteriores de ese correo (RF-006).
- La lista del club muestra, por cada correo, su invitación más reciente.
- Solo se reenvía o cancela una invitación `PENDIENTE`.
- No se crea una invitación para un correo que ya es integrante del club, aprobado o en espera,
  ni para el correo de la cuenta DESARROLLADOR (RF-007).
- Una invitación del club no permite consultar, registrarse ni aceptar mientras su club está
  `DADO_DE_BAJA`; sí mientras está `SUSPENDIDO` (RF-030). La suspensión no alarga `VenceEn`.
- Las invitaciones se borran con el rechazo de la persona que las usó (ver arriba) y, como hasta
  ahora, al eliminar el club.

## Enumeraciones

```text
EstadoInvitacion  PENDIENTE · USADA · VENCIDA · CANCELADA      (nueva; derivada, no se guarda)
EstadoIngreso     EN_ESPERA · APROBADO                         (sin cambios; EN_ESPERA empieza a usarse)
```

## Lo que no se guarda

- **El rechazo**: no deja fila ni marca.
- **La categoría y la mensualidad del jugador aprobado**: llegan con sus funcionalidades. Esta
  deja guardados la fecha de nacimiento y los datos de registro que necesitarán.
