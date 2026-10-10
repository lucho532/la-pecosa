# Modelo de datos: Ficha del jugador

**Funcionalidad**: `005-ficha-jugador` | **Fecha**: 2026-10-09

Esta funcionalidad crea **dos tablas** y **dos enumeraciones**, con una migración
(`FichaDelJugador`). No añade columnas a las tablas que ya existen: cambia quién puede escribir
algunas de ellas. Los modelos anteriores están en la
[003](../003-categorias-club/data-model.md) y la [004](../004-invitacion-con-rol/data-model.md);
los motivos de cada decisión, en [research.md](research.md).

## De dónde sale cada grupo de la ficha

| Grupo (RF-001) | Dónde vive | Novedad |
| --- | --- | --- |
| Identidad: nombres, apellidos, tipo y número de documento, fecha de nacimiento | `UsuarioRol` | Ya existe; ahora se puede cambiar |
| Identidad: categoría y equipos | `UsuarioRol.CategoriaId`, `JugadorEquipo` | Ya existe; solo se lee |
| Contacto: correo, celular, nombre del responsable | `Usuario` | Ya existe; celular y responsable se pueden cambiar |
| Contacto de emergencia, seguridad social, datos clínicos, último cambio | **`FichaJugador`** | Nueva |
| Documentos | **`DocumentoJugador`** | Nueva |

## FichaJugador (nueva)

Lo que la ficha añade a un jugador en un club. Tabla `FichasJugador`. Implementa
`IPerteneceAClub`: el filtro de aislamiento se le aplica solo.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| UsuarioRolId | Guid | Clave primaria y foránea a `UsuarioRol`: una ficha por jugador y club. Borrado en cascada |
| ClubId | Guid | Foránea a `Club`, borrado en cascada. Siempre el club del jugador |
| EmergenciaNombre | texto, máx. 160 | Opcional |
| EmergenciaParentesco | texto, máx. 40 | Opcional |
| EmergenciaCelular | texto, máx. 20 | Opcional |
| EntidadSalud | texto, máx. 120 | Opcional. Entidad de salud a la que está afiliado |
| LugarAtencion | texto, máx. 200 | Opcional. Dónde lo atienden |
| GrupoSanguineo | `GrupoSanguineo` | Opcional. Se guarda como texto |
| Alergias | texto, máx. 1000 | Opcional |
| Enfermedades | texto, máx. 1000 | Opcional. Enfermedades o condiciones |
| Medicamentos | texto, máx. 1000 | Opcional |
| Observaciones | texto, máx. 1000 | Opcional |
| UltimoCambioEn | fecha y hora UTC | Obligatorio: la fila solo existe desde el primer cambio |
| UltimoCambioPorUsuarioId | Guid | Opcional. Foránea a `Usuario`; pasa a nulo si esa cuenta se elimina |
| UltimoCambioPorNombre | texto, máx. 161 | Obligatorio. Nombres y apellidos de quien cambió, copiados en ese momento (§13) |

**Reglas**:

- Solo existe para un integrante con rol JUGADOR e ingreso `APROBADO`, activo o retirado (RF-004).
- La fila se crea con el primer cambio de la ficha, sea cual sea. Sin fila, la ficha se lee vacía
  y sin último cambio (escenario 1.10).
- Todos los datos de contacto de emergencia, seguridad social y clínicos son opcionales; una fila
  con todos vacíos es válida (RF-002). Un texto vacío o solo con espacios se guarda como nulo.
- Los cuatro datos clínicos de texto y el grupo sanguíneo nunca salen por la API para un
  DIRECTIVO (RF-010).
- Los tres datos del último cambio se escriben juntos y sustituyen a los anteriores. Los sella
  cualquier cambio de la ficha: contacto, salud, documento de identidad, identidad o un archivo
  (RF-038). No hay historial (§18).
- Retirar o reincorporar al jugador no toca la fila (RF-036).

## DocumentoJugador (nueva)

El archivo entregado para uno de los documentos pedidos. Tabla `DocumentosJugador`. Implementa
`IPerteneceAClub`.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| UsuarioRolId | Guid | Parte de la clave primaria. Foránea a `UsuarioRol`, borrado en cascada |
| Documento | `DocumentoPedido` | Parte de la clave primaria. Se guarda como texto |
| ClubId | Guid | Foránea a `Club`, borrado en cascada. Siempre el club del jugador |
| Contenido | bytes | Obligatorio. Máximo 10 MB |
| TipoContenido | texto, máx. 30 | `application/pdf`, `image/jpeg`, `image/png` o `image/webp`, según la firma del archivo |
| TamanoBytes | entero | Obligatorio. Para mostrar el tamaño sin leer el contenido |
| SubidoEn | fecha y hora UTC | Obligatorio. Fecha del archivo vigente (RF-028) |

**Reglas**:

- La clave primaria `(UsuarioRolId, Documento)` garantiza un solo archivo por documento pedido.
  Subir otro sustituye el contenido, el tipo, el tamaño y la fecha en la misma fila: el anterior
  deja de existir (RF-029).
- "Entregado" es que la fila existe; "pendiente", que no existe. No hay columna de estado ni
  revisión por parte del club (fuera de alcance).
- No se guarda el nombre original del archivo.
- Ninguna consulta de listas o de estado lee `Contenido`.
- No se puede borrar un documento sin borrar al jugador (supuesto de la spec).

## Enumeraciones nuevas

```text
DocumentoPedido                    GrupoSanguineo
  COPIA_DOCUMENTO_IDENTIDAD          A_POSITIVO   A_NEGATIVO
  CERTIFICADO_SALUD                  B_POSITIVO   B_NEGATIVO
                                     AB_POSITIVO  AB_NEGATIVO
                                     O_POSITIVO   O_NEGATIVO
```

`DocumentoPedido` es la lista fija de RF-027: igual para todos los clubes y no ampliable desde la
aplicación. No debe confundirse con `TipoDocumento`, que es el tipo del documento de identidad
(§10).

## UsuarioRol (cambia de uso, no de columnas)

| Campo | Antes | Ahora |
| --- | --- | --- |
| TipoDocumento, NumeroDocumento | Se fijaban al registrarse y no cambiaban | Los cambian la cuenta del jugador y el PRESIDENTE (RF-016, RF-018). Sigue siendo único por club, contando a los retirados (RF-023) |
| Nombres, Apellidos, FechaNacimiento | Se fijaban al registrarse y no cambiaban | Solo los corrige el PRESIDENTE (RF-017, RF-018). La fecha no puede ser futura (RF-021) |
| CategoriaId | La ponen la ubicación automática y el PRESIDENTE | Además, corregir la fecha de nacimiento de un jugador activo sin categoría lo ubica en la categoría activa del año nuevo (RF-026). Nunca mueve a quien ya tiene categoría (RF-025) |

Cambiar el documento o corregir la identidad no crea otra fila ni cambia `Id`, `UsuarioId`,
`CategoriaId` (salvo la ubicación anterior), los equipos, el estado de ingreso ni el retiro
(RF-022).

## Usuario (cambia de uso, no de columnas)

| Campo | Antes | Ahora |
| --- | --- | --- |
| Celular | Se fijaba al registrarse | Lo cambian, desde la ficha de un jugador de la cuenta, la propia cuenta y el PRESIDENTE del club de ese jugador. Obligatorio |
| NombreResponsable | Se fijaba al registrarse o al aceptar una invitación de JUGADOR | Igual que el celular. Obligatorio si el jugador de esa ficha es menor de 18 años ese día (RF-020) |
| Correo | No cambia | No cambia desde la ficha (RF-018) |

Son los dos únicos datos que un club puede cambiar con efecto en otro (RF-039): al ser de la
cuenta, el cambio se ve en todos los jugadores y clubes de esa persona.

## Relaciones

```text
Club 1 ──── * UsuarioRol 1 ──── 0..1 FichaJugador
                   │ 1
                   └──────────── 0..2 DocumentoJugador   (uno por DocumentoPedido)

Usuario 1 ──── * UsuarioRol          (celular y responsable, comunes a todos sus jugadores)
Usuario 1 ──── * FichaJugador        (quien hizo el último cambio; opcional)
```

## Qué ve cada rol

La decide `ReglaAccesoAFicha` (research §2). "No" significa que el dato no viaja en la respuesta.

| Dato | PRESIDENTE | DIRECTIVO | ENTRENADOR (de su categoría) | Cuenta del jugador |
| --- | --- | --- | --- | --- |
| Identidad y contacto | Sí | Sí | Sí | Sí |
| Contacto de emergencia y seguridad social | Sí | Sí | Sí | Sí |
| Datos clínicos | Sí | No | Sí | Sí |
| Documentos (estado y archivo) | Sí | Sí | No | Sí |
| Último cambio | Sí | Sí | Sí | Sí |
| Jugadores sin categoría | Sí | Sí | No | El suyo |
| Jugadores retirados | Sí | Sí | No | No (no entra al club) |

## Borrado y conservación

| Qué ocurre | FichaJugador y DocumentoJugador |
| --- | --- |
| Retirar o reincorporar a un jugador | Se conservan intactos (RF-036) |
| Rechazar un ingreso en espera, eliminar a alguien del club, eliminar una cuenta sin club | Se borran en cascada con su `UsuarioRol` (RF-037) |
| Eliminar el club | Se borran en cascada con el club (RF-037) |
| Se elimina la cuenta de quien hizo el último cambio | Se conserva el nombre copiado; la referencia queda nula |

## Lo que no se guarda

- Ningún historial de cambios de la ficha ni el valor anterior de ningún dato (RF-038, §18).
- Ninguna versión anterior de un archivo ni su nombre original.
- Ningún contador de documentos pendientes: se calcula al consultar (CE-009).
- Ninguna copia del celular ni del responsable en la ficha.
- Ningún dato de la ficha para PRESIDENTE, DIRECTIVO o ENTRENADOR (RF-004).
