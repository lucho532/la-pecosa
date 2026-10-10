# Modelo de datos: Agregar un hermano y elegir el jugador

**Funcionalidad**: `006-agregar-hermano` | **Fecha**: 2026-10-09

Esta funcionalidad **no crea tablas**. Añade una columna a `UsuariosRol`, con una migración
(`HermanosDeLaCuenta`), y cambia una suposición: una cuenta puede tener varios integrantes en el
mismo club. Los modelos anteriores están en la [003](../003-categorias-club/data-model.md), la
[004](../004-invitacion-con-rol/data-model.md) y la [005](../005-ficha-jugador/data-model.md); los
motivos de cada decisión, en [research.md](research.md).

## Cómo se corresponden las entidades de la spec

| Entidad de la spec | Dónde vive | Novedad |
| --- | --- | --- |
| Cuenta | `Usuario` | Sin cambios: correo, contraseña, celular y responsable |
| Jugador | `UsuarioRol` con rol JUGADOR | Puede haber varios con el mismo `UsuarioId` y `ClubId` |
| Estado de ingreso | `UsuarioRol.EstadoIngreso` | Sin cambios: `EN_ESPERA` vuelve a usarse |
| Jugador de origen | **`UsuarioRol.AgregadoDesdeUsuarioRolId`** | Columna nueva |
| Jugador elegido | No se guarda | Cabecera `X-Jugador-Elegido` en cada petición |
| Sesión limitada a un jugador | No se guarda | Reclamación `jugadores` del token |

## UsuarioRol (cambia)

| Campo | Tipo | Reglas |
| --- | --- | --- |
| AgregadoDesdeUsuarioRolId | Guid, opcional | **Nuevo**. Foránea a `UsuarioRol` (la misma tabla), borrado a `NULL`. Solo lo tiene el hermano agregado desde una ficha; `NULL` en quien entró con invitación |

Sin índice nuevo: solo se lee junto con la sala de espera, que ya filtra por
`ClubId, EstadoIngreso`.

**Índices que ya existen y sostienen esta funcionalidad**:

- `ClubId, NumeroDocumento`, único (`IndicesUnicos.DocumentoEnClub`): impide un hermano con un
  documento repetido en el club, en cualquier estado (RF-005).
- `ClubId, UsuarioId`, **no único**: sirve para leer los integrantes de una cuenta en un club. Se
  deja como está.

### Cómo nace un hermano

| Campo | Valor |
| --- | --- |
| UsuarioId, ClubId | Los del jugador de origen |
| Rol | `JUGADOR` |
| EstadoIngreso | `EN_ESPERA` |
| Nombres, Apellidos, TipoDocumento, NumeroDocumento, FechaNacimiento | Los que escribe la familia, normalizados como en el registro |
| AgregadoDesdeUsuarioRolId | El jugador de origen |
| CategoriaId | `NULL` |
| Activo | `true` |
| AprobadoEn, AprobadoPor…, RolDeIngreso | `NULL` hasta que se apruebe |
| CreadoEn | El momento en que se agregó |

No se crea `FichaJugador` ni `DocumentoJugador`: la ficha no tiene fila hasta su primer cambio
(005).

### Reglas de validación

| Regla | Requisito | Respuesta |
| --- | --- | --- |
| Nombres, apellidos, tipo y número de documento y fecha de nacimiento, obligatorios y con el formato del registro | RF-002, RF-006 | `400 datos_invalidos` |
| Fecha de nacimiento no futura | RF-006 | `400 datos_invalidos` |
| Hermano menor y cuenta sin responsable: `nombreResponsable` obligatorio | RF-004 | `400 datos_invalidos` |
| Documento que ya tiene otro integrante del club | RF-005 | `409 documento_repetido_en_club` |
| Documento que usa otra cuenta en otro club (supuesto 1) | — | `409 documento_en_otra_cuenta` |
| Documento de un jugador en espera de la misma cuenta en el club (supuesto 2) | RF-009 | `200` con el que ya existe |

### Transiciones del estado de ingreso

```text
(no existe) ──agregar hermano──▶ EN_ESPERA ──aprobar──▶ APROBADO
                                     │
                                     └──rechazar──▶ (no existe)
```

- **Aprobar** (sin cambios desde la 004): `APROBADO`, rol JUGADOR, `AprobadoEn` y quién lo
  aprobó; se ubica en la categoría activa de su año o queda sin categoría.
- **Rechazar**: se borra la fila. No queda estado ni registro. La cuenta y sus demás integrantes
  no se tocan. Las invitaciones del club a ese correo solo se borran si a la cuenta no le queda
  ningún integrante en el club (cambio respecto de la 002).
- Retirar y reincorporar siguen siendo de cada jugador por separado (`Activo`).

## Usuario (sin cambios de estructura)

`NombreResponsable` puede escribirse al agregar un hermano menor cuando la cuenta no lo tenía
(RF-004). Es el mismo dato de la cuenta que ya cambia la ficha (005, RF-039).

## Lo que no se guarda

- **El jugador elegido**: viaja en la cabecera `X-Jugador-Elegido` y solo vale si es un
  integrante de la cuenta de la sesión en el club de la ruta.
- **La limitación de la sesión con documento**: viaja en la reclamación `jugadores` del token,
  con los identificadores de los integrantes de la cuenta que tenían ese número al entrar.

## Borrados en cascada

No cambian. Rechazar a un hermano, eliminar la cuenta o eliminar el club borran sus filas de
`UsuarioRol` y, con ellas, su ficha, sus documentos y sus equipos. Borrar al jugador de origen
deja `AgregadoDesdeUsuarioRolId` en `NULL` en sus hermanos y no los borra.
