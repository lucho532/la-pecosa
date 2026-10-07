# Modelo de datos: Base multiclub y panel de administración de la plataforma

**Funcionalidad**: `001-base-multiclub` | **Fecha**: 2026-10-07

Todas las claves primarias son `Guid` (versión 7) generados por la aplicación. Las fechas se
guardan en UTC. Las relaciones usan siempre identificadores internos, nunca el documento (§10).

## Vista general

```text
Usuario 1 ──── N UsuarioRol N ──── 1 Club 1 ──── 0..1 EscudoClub
   │                                    │
   ├── 0..1 FotoPerfil                  └── N Invitacion
   └── N SolicitudRecuperacion
```

- **Entidades de la plataforma** (sin `ClubId`, fuera del filtro de aislamiento): `Club`,
  `Usuario`, `FotoPerfil`, `SolicitudRecuperacion`.
- **Entidades de club** (implementan `IPerteneceAClub`, con filtro global y borrado en cascada
  desde `Club`): `UsuarioRol`, `Invitacion`, `EscudoClub`.

## Club

Escuela o equipo que usa la plataforma.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| Nombre | texto (120) | Obligatorio |
| NombreNormalizado | texto (120) | Minúsculas, sin espacios sobrantes. Índice único (RF-007) |
| Sede | texto (120) | Opcional |
| Direccion | texto (200) | Opcional |
| CorreoContacto | texto (254) | Opcional; formato de correo |
| TelefonoContacto | texto (20) | Opcional |
| ColorPrincipal | texto (7) | Opcional; `#RRGGBB`. Nulo = identidad neutra |
| ColorAcento | texto (7) | Opcional; `#RRGGBB` |
| VersionEscudo | entero | Sube cada vez que cambia el escudo; 0 = sin escudo |
| Estado | `EstadoClub` | Obligatorio; `ACTIVO` al crearse |
| EstadoCambiadoPorUsuarioId | Guid | Quién hizo el último cambio de estado (RF-031) |
| EstadoCambiadoEn | fecha y hora | Cuándo (RF-031) |
| CreadoEn | fecha y hora | Obligatorio |

Solo se guarda el último cambio de estado: §18 no permite un historial de estados.

**Quién edita qué**: nombre, sede, dirección y contacto, el DESARROLLADOR y el PRESIDENTE del
club (RF-010). Colores y escudo, solo el DESARROLLADOR (RF-008). Estado, solo el DESARROLLADOR.

### Transiciones de `EstadoClub`

```text
            suspender                  dar de baja
  ACTIVO ───────────────▶ SUSPENDIDO ───────────────▶ DADO_DE_BAJA ──eliminar──▶ (borrado)
     ▲ ◀── levantar ──────────┘                            │
     │                                                      │
     ├──────────────── dar de baja ────────────────────────▶│
     └◀─────────────── revertir la baja ────────────────────┘
```

- Cualquier otra transición se rechaza con `409`.
- Eliminar solo es posible desde `DADO_DE_BAJA` (RF-029) y exige escribir el nombre del club.
- Ninguna transición, salvo eliminar, modifica otros datos del club (RF-026, CE-007).

### Efecto del estado sobre el acceso

| Estado | PRESIDENTE | Resto de integrantes |
| --- | --- | --- |
| ACTIVO | Entra | Entran |
| SUSPENDIDO | Entra y ve que está suspendido | Ven el aviso de incidencia temporal |
| DADO_DE_BAJA | No entra | No entran |

Se comprueba en cada petición (RF-030).

## EscudoClub

Imagen del escudo, separada de `Club` para no cargarla en cada consulta.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| ClubId | Guid | Clave primaria y foránea a `Club`, con cascada |
| Contenido | binario | Máximo 1 MB |
| TipoContenido | texto (30) | `image/png`, `image/jpeg` o `image/webp`, según la firma del archivo |

## Usuario

La cuenta con la que se inicia sesión. Es una sola aunque la persona pertenezca a varios clubes.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| Correo | texto (254) | Obligatorio |
| CorreoNormalizado | texto (254) | Minúsculas y sin espacios en los extremos. Índice único (RF-002) |
| ContrasenaHash | texto | Nulo solo en la cuenta DESARROLLADOR recién creada. Nunca sale por la API (RF-019) |
| Celular | texto (20) | Obligatorio al registrarse |
| EsDesarrollador | booleano | Índice único parcial sobre `true`: como máximo una cuenta (RF-001) |
| IntentosFallidos | entero | 0 a 5 |
| Bloqueada | booleano | `true` al quinto fallo seguido (RF-005) |
| SelloSeguridad | Guid | Cambia al cambiar la contraseña; invalida los tokens anteriores |
| VersionFoto | entero | Sube cada vez que cambia la foto de perfil; 0 = sin foto |
| CreadoEn | fecha y hora | Obligatorio |

**Reglas**:

- Una cuenta sin `ContrasenaHash` o con `Bloqueada = true` no puede iniciar sesión.
- Restablecer la contraseña pone `IntentosFallidos = 0`, `Bloqueada = false` y un `SelloSeguridad`
  nuevo (RF-005a).
- La cuenta DESARROLLADOR no tiene ningún `UsuarioRol` y su correo no admite invitaciones
  (RF-001, `correo_del_desarrollador`). Las demás tienen siempre al menos uno: una
  cuenta que pierde su último integrante se elimina (RF-019a, supuesto de la spec).

## FotoPerfil

Foto de perfil de una cuenta, separada de `Usuario` para no cargarla en cada consulta. Es
opcional y la misma en todos los clubes de la persona.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| UsuarioId | Guid | Clave primaria y foránea a `Usuario`, con cascada |
| Contenido | binario | Máximo 1 MB |
| TipoContenido | texto (30) | `image/png`, `image/jpeg` o `image/webp`, según la firma del archivo |

Solo la carga, la cambia, la quita y la ve su dueño (RF-036 a RF-038). Se borra con la cuenta.

## UsuarioRol

Pertenencia de una persona a un club, con su único rol en él y su identidad en ese club. Es la
entidad de la constitución §12.3 y lo que la spec llama "integrante".

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| ClubId | Guid | Foránea a `Club`, con cascada |
| UsuarioId | Guid | Foránea a `Usuario` |
| Rol | `Rol` | `PRESIDENTE`, `DIRECTIVO`, `ENTRENADOR` o `JUGADOR`. Nunca `DESARROLLADOR` |
| EstadoIngreso | `EstadoIngreso` | `EN_ESPERA` o `APROBADO`. En esta funcionalidad, siempre `APROBADO` (RF-016) |
| Nombres | texto (80) | Obligatorio |
| Apellidos | texto (80) | Obligatorio |
| TipoDocumento | `TipoDocumento` | Obligatorio (§10) |
| NumeroDocumento | texto (20) | Obligatorio; sin espacios ni puntos y en minúsculas (RF-002) |
| FechaNacimiento | fecha | Obligatoria; no futura |
| CreadoEn | fecha y hora | Obligatorio |

**Índices y reglas**:

- Único `(ClubId, NumeroDocumento)`: el documento no se repite dentro de un club (RF-017).
- Una persona es única y tiene un único inicio de sesión: un `NumeroDocumento` solo puede estar
  bajo un mismo `UsuarioId` en toda la plataforma (RF-017a). Lo comprueba el servicio de registro.
- Un integrante tiene un único rol (RF-021); cambiarlo reemplaza el valor. Aceptar una invitación
  a un club del que la cuenta ya es integrante reemplaza el rol de ese integrante (RF-018).
- **Último presidente** (RF-020, RF-019a): no se puede quitar el rol ni eliminar a un integrante
  PRESIDENTE si es el único del club con ese rol. Las invitaciones pendientes no cuentan. La regla
  vive en `Dominio/Reglas` y se comprueba dentro de la misma transacción que el cambio.
- Quitar el rol de presidente con "asignar otro rol" solo admite `DIRECTIVO` o `ENTRENADOR`.
- "Eliminar del club" borra físicamente el integrante. Está justificado según §14: lo pide la
  spec y en esta funcionalidad el integrante no tiene todavía historial deportivo ni financiero.

## Invitacion

Enlace enviado por correo para registrarse en un club con un rol.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| ClubId | Guid | Foránea a `Club`, con cascada |
| Rol | `Rol` | En esta funcionalidad, siempre `PRESIDENTE` (RF-012) |
| Correo | texto (254) | Obligatorio; se guarda normalizado |
| TokenHash | texto (64) | SHA-256 del token. Índice único. El token en claro no se guarda |
| EstadoEnvio | `EstadoEnvio` | `PENDIENTE`, `ENVIADO` o `FALLIDO` |
| CreadaPorUsuarioId | Guid | Quién la envió |
| CreadaEn | fecha y hora | Obligatorio |
| VenceEn | fecha y hora | `CreadaEn` + 7 días |
| UsadaEn | fecha y hora | Nulo hasta que se usa |
| AnuladaEn | fecha y hora | Nulo hasta que otra la reemplaza |

**Estado derivado** (no se guarda):

```text
vigente  = UsadaEn es nulo  y  AnuladaEn es nulo  y  ahora < VenceEn
usada    = UsadaEn tiene valor
anulada  = AnuladaEn tiene valor
vencida  = ninguna de las anteriores
```

**Reglas**:

- Solo una invitación vigente permite registrarse (RF-013, RF-014).
- El registro usa siempre el correo de la invitación; el servidor ignora cualquier otro (RF-013).
- Reenviar o corregir el correo anula la invitación y crea otra (RF-012).
- Usar la invitación y crear el integrante ocurren en la misma transacción.
- "El presidente aún no se ha registrado" es un dato derivado: el club no tiene ningún integrante
  con rol `PRESIDENTE`.

## SolicitudRecuperacion

Enlace de un solo uso para crear una contraseña nueva.

| Campo | Tipo | Reglas |
| --- | --- | --- |
| Id | Guid | Clave primaria |
| UsuarioId | Guid | Foránea a `Usuario`, con cascada |
| TokenHash | texto (64) | SHA-256 del token. Índice único |
| CreadaEn | fecha y hora | Obligatorio |
| VenceEn | fecha y hora | `CreadaEn` + 60 minutos |
| UsadaEn | fecha y hora | Nulo hasta que se usa |

Pedir una recuperación nueva deja sin efecto las anteriores de ese usuario.

## Enumeraciones

```text
Rol            DESARROLLADOR · PRESIDENTE · DIRECTIVO · ENTRENADOR · JUGADOR
EstadoClub     ACTIVO · SUSPENDIDO · DADO_DE_BAJA
EstadoIngreso  EN_ESPERA · APROBADO
EstadoEnvio    PENDIENTE · ENVIADO · FALLIDO
TipoDocumento  REGISTRO_CIVIL · TARJETA_IDENTIDAD · CEDULA_CIUDADANIA · CEDULA_EXTRANJERIA
```

Se guardan como texto en la base de datos.

## Lo que no se guarda en el servidor

- **Preferencia de tema**: vive en el almacenamiento local del dispositivo (RF-033).
- **Club elegido**: va en la dirección de la pantalla; el frontend recuerda el último en el
  almacenamiento local.
