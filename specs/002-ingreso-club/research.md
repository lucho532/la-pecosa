# Investigación: Ingreso de personas al club

**Funcionalidad**: `002-ingreso-club` | **Fecha**: 2026-10-07

Este documento resuelve las incógnitas técnicas del plan. Las tecnologías, el aislamiento por
`ClubId`, la sesión, el correo y las pruebas son los de la funcionalidad 001
([research.md de la 001](../001-base-multiclub/research.md)) y no se repiten aquí. Cada decisión
indica qué se eligió, por qué y qué se descartó. Las marcadas con **(supuesto)** rellenan un
detalle que la spec no fija; están reunidas al final para que el propietario las confirme.

## 1. Cómo se distingue una invitación del club de una de presidente

- **Decisión**: se reutiliza la tabla `Invitacion` sin columnas nuevas. La invitación del club se
  guarda con `Rol = JUGADOR`; la de presidente, con `Rol = PRESIDENTE`. El estado de ingreso con el
  que queda quien la usa se deriva del rol: `PRESIDENTE` → `APROBADO`; cualquier otro →
  `EN_ESPERA`. La derivación vive en un único lugar, `Dominio/Reglas/ReglaIngresoPorInvitacion`.
- **Motivo**: RF-003 dice que la invitación del club no lleva rol y que quien la usa entra como
  JUGADOR y en espera; §8 dice que JUGADOR es el rol por defecto. Con el rol ya basta para saber
  de qué tipo es (§19).
- **Alternativas**: una columna `Origen` o `PasaPorSalaDeEspera` (duplica lo que el rol ya dice);
  una tabla aparte de invitaciones del club (dos tablas para el mismo concepto).

## 2. Las invitaciones de cada lado no se mezclan

- **Decisión**: el club solo ve, reenvía, cancela y anula invitaciones con rol distinto de
  `PRESIDENTE`; el panel del DESARROLLADOR, solo las de `PRESIDENTE`. Para ello:
  - El repositorio nuevo `RepositorioInvitacionesClub` trabaja con el filtro de aislamiento activo
    y añade siempre `Rol != PRESIDENTE`.
  - `RepositorioInvitacionesPlataforma` (001) pasa a añadir `Rol == PRESIDENTE` en sus tres
    consultas. Hoy lista todas las del club, y con esta funcionalidad eso dejaría ver al
    DESARROLLADOR los correos invitados por el club.
- **Motivo**: RF-029 (el DESARROLLADOR no ve invitaciones que no sean de presidente) y evitar que
  un DIRECTIVO anule la invitación de un presidente al invitar a su mismo correo.
- **Consecuencia**: un mismo correo puede tener a la vez una invitación de presidente y una del
  club. No hace daño: si usa primero la del club queda en espera, y al usar después la de
  presidente pasa a PRESIDENTE aprobado (regla RF-018 de la 001, que no cambia).

## 3. Estados de la invitación en la lista del club

- **Decisión**: el estado se sigue derivando, no se guarda. Se añade a `Dominio` la enumeración
  `EstadoInvitacion` (`PENDIENTE`, `USADA`, `VENCIDA`, `CANCELADA`) y el método
  `Invitacion.EstadoEn(ahora)`. Cancelar usa la columna `AnuladaEn` que ya existe.
- **Qué muestra la lista** **(supuesto 1)**: una fila por correo, la de su invitación más
  reciente. Una invitación reemplazada por un reenvío nunca es la más reciente, así que no
  aparece; una cancelada aparece como "cancelada" hasta que se vuelve a invitar a ese correo.
- **Motivo**: RF-005 pide cuatro estados y el escenario 1.5 pide que la cancelada se vea como tal.
  Con "la más reciente por correo" se distingue cancelada de reemplazada sin otra columna y la
  lista no se llena de enlaces viejos.
- **Reenviar y cancelar**: solo sobre una invitación `PENDIENTE` (RF-006). A una vencida o
  cancelada se la vuelve a invitar con el mismo formulario, que crea una nueva.
- **Quién la envió**: se muestra el nombre del integrante del club cuya cuenta es
  `CreadaPorUsuarioId`. Si esa persona ya no está en el club, el campo va vacío. La invitación
  sigue sirviendo (caso límite de la spec).
- **Alternativas**: columna `CanceladaEn` y listar todo el historial (más datos y una lista que
  solo crece).

## 4. La sala de espera se aplica en la autorización, para todo el club

- **Decisión**: `IntegranteDelClubAttribute` niega **por defecto** cualquier endpoint
  `/api/clubes/{clubId}/**` a un integrante `EN_ESPERA`, con `403` y código `ingreso_en_espera`.
  No se abre ningún endpoint de club para las cuentas en espera. La regla se añade a
  `ReglaAccesoPorEstado`, que pasa a recibir también el estado de ingreso, con un resultado nuevo
  `IngresoEnEspera`.
- **De dónde sale la pantalla de espera**: de la sesión. `GET /api/sesion` ya devuelve, por cada
  club de la cuenta, su nombre y su identidad (excepción "consultas de la propia cuenta" de
  §7.1); se le añade `estadoIngreso`. Con eso el frontend pinta la sala de espera con el nombre y
  los colores del club sin pedir nada al club.
- **Motivo**: RF-016 exige que la restricción esté en el servidor y cubra todas las operaciones.
  Al estar en el atributo que ya protege todo `/api/clubes/**`, las funcionalidades futuras quedan
  cubiertas sin acordarse de nada. Como el atributo consulta la base de datos en cada petición,
  la aprobación y el rechazo se aplican a las sesiones abiertas (RF-024) sin más mecanismos.
- **Orden de las comprobaciones**: primero el estado del club y después el estado de ingreso. Una
  cuenta en espera de un club suspendido recibe `club_suspendido` y ve el aviso de incidencia
  temporal, como cualquier integrante que no es presidente (§7.4) **(supuesto 2)**.
- **Prueba que lo vigila**: una prueba de integración recorre todos los endpoints
  `/api/clubes/{clubId}/**` de la API (ya existe `EndpointsDeLaApi` en las pruebas) y comprueba
  que una cuenta en espera recibe `403` en todos (CE-005).
- **Alternativas**: un atributo aparte que cada controlador debe recordar poner (falla abierto);
  un endpoint propio para la sala de espera (innecesario: la sesión ya trae lo que hace falta).

## 5. Varios roles en la autorización

- **Decisión**: `IntegranteDelClubAttribute` pasa de aceptar un rol a aceptar una lista
  (`[IntegranteDelClub(Rol.PRESIDENTE, Rol.DIRECTIVO)]`). Todos los endpoints de "Ingresos" la
  usan.
- **Motivo**: RF-001, RF-020, RF-021 y RF-027 autorizan a los mismos dos roles. En un club
  suspendido el DIRECTIVO ya no pasa la comprobación de estado, así que RF-030 se cumple sin
  código nuevo.

## 6. Registro: responsable y menores de edad

- **Decisión**: el registro añade el campo `nombreResponsable`. Es obligatorio cuando la persona
  es menor de 18 años el día del registro; para un adulto es opcional (RF-010). La comprobación
  vive en `Dominio/Reglas/ReglaMayoriaDeEdad` y la aplica `ValidadorRegistro` en el servidor.
- **Dónde se guarda**: en `Usuario.NombreResponsable`, junto a `Celular`. §12.1.2 dice que los
  hermanos comparten "correo, celular y responsable" de la cuenta, así que es un dato de contacto
  de la cuenta y no de cada integrante.
- **Qué día es "hoy"** **(supuesto 3)**: la fecha UTC del servidor, la misma que ya se usa para
  rechazar fechas de nacimiento futuras. Solo cambia el resultado durante las cinco horas de
  diferencia con Colombia en el día exacto en que alguien cumple 18.
- **Aplica a todo registro**, también al de un presidente, que casi siempre será adulto.
- **RF-011**: la pantalla explica que un jugador se registra con el documento del jugador y el
  correo de su acudiente, y un entrenador o directivo con su propio documento. Es solo texto.

## 7. Persona que ya tiene cuenta (RF-013)

- **Decisión**: se reutiliza `POST /api/invitaciones/aceptacion` de la 001. Con una invitación del
  club, el integrante nuevo se crea con rol `JUGADOR` y `EN_ESPERA`, copiando la identidad del
  integrante más reciente de la cuenta. No se le piden datos de nuevo.
- **Cambio necesario**: hoy, si la cuenta ya es integrante de ese club, aceptar una invitación le
  reemplaza el rol por el de la invitación. Con una invitación del club eso degradaría a un
  presidente o directivo a jugador en espera. Por eso, con una invitación del club y un integrante
  que ya existe, la respuesta es `409 ya_perteneces_al_club` y no cambia nada. La regla de la 001
  se conserva solo para invitaciones de presidente.
- **Motivo**: RF-007 impide invitar a quien ya está en el club, pero la persona pudo entrar por
  otra vía entre el envío y el uso de la invitación.

## 8. Invitar desde el club

- **Decisión**: servicio nuevo `ServicioInvitacionesClub`, con un repositorio que trabaja dentro
  del club de la petición. La construcción de la invitación (token de 32 bytes, hash SHA-256,
  vencimiento a 7 días) se extrae de `ServicioInvitacionPresidente` a una utilidad compartida para
  no duplicarla. El correo se envía después de confirmar la transacción; si falla, la invitación
  queda con `EstadoEnvio = FALLIDO` y la respuesta lo dice para que la pantalla avise (RF-008).
- **Rechazos** (RF-007), todos `409`: `ya_esta_en_el_club` (el correo es de un integrante
  aprobado), `ya_esta_en_espera` y `correo_del_desarrollador`.
- **Texto del correo y de la pantalla**: la invitación del club no nombra ningún rol. La plantilla
  del correo y el subtítulo de la pantalla de invitación dejan de decir "para ser jugador".
  `InvitacionVigenteDto` gana `pasaPorSalaDeEspera` para que el frontend no tenga que deducirlo.
- **Club dado de baja**: nadie del club pasa la autorización, así que no se puede invitar, aprobar
  ni rechazar. Además, consultar, registrarse o aceptar con una invitación **del club** responde
  `410 invitacion_no_valida` mientras dure la baja (caso límite de la spec). Las invitaciones de
  presidente no cambian de comportamiento **(supuesto 4)**.

## 9. Aprobación

- **Decisión**: `POST /api/clubes/{clubId}/ingresos/{usuarioRolId}/aprobacion` con el rol
  elegido. Qué rol puede asignar cada quien vive en `Dominio/Reglas/ReglaAprobacionIngreso`:

  | Quien aprueba | Roles que puede dejar |
  | --- | --- |
  | PRESIDENTE | JUGADOR, ENTRENADOR, DIRECTIVO |
  | DIRECTIVO | JUGADOR, ENTRENADOR |

  `PRESIDENTE` nunca es asignable (RF-023). Aprobarse a uno mismo se rechaza (RF-021).
- **Una sola vez** (RF-026): el cambio es una única sentencia condicionada,
  `UPDATE ... WHERE Id = @id AND EstadoIngreso = 'EN_ESPERA'`. Si no modifica ninguna fila, el
  ingreso ya no estaba pendiente: `409 ingreso_ya_aprobado` si el integrante existe, `404` si fue
  rechazado. La pantalla muestra el mensaje y recarga la sala de espera. Así se resuelven también
  "dos aprueban a la vez" y "uno aprueba y otro rechaza".
- **Excepción del DIRECTIVO** (§8, §12.2): el DIRECTIVO solo puede asignar ENTRENADOR dentro de la
  misma operación de aprobar. No existe ningún endpoint para cambiar el rol de alguien ya
  aprobado, así que el escenario 3.5 se cumple por construcción.
- **Fuera de esta funcionalidad**: asignar categoría, generar la mensualidad (§12.1.1) y eliminar
  la ficha de Jugador al pasar a ENTRENADOR o DIRECTIVO (§12.2). Las entidades Categoría, Cargo y
  Jugador todavía no existen; lo dicen los supuestos de la spec.

## 10. Registro de la aprobación

- **Decisión**: cuatro columnas nuevas en `UsuarioRol`: `AprobadoEn`, `AprobadoPorUsuarioId`,
  `AprobadoPorNombre` y `RolDeIngreso`. La lista de ingresos aprobados son los integrantes del
  club con `AprobadoEn` no nulo, del más reciente al más antiguo.
- **Por qué se copian el nombre y el rol**: §13. Quien aprobó puede dejar el club y el rol del
  integrante puede cambiar más adelante; la lista debe seguir diciendo quién aprobó y con qué rol
  entró. `AprobadoPorUsuarioId` es una clave foránea que se pone a nulo si esa cuenta se elimina.
- **Quién no aparece**: los presidentes que entraron con una invitación del DESARROLLADOR, porque
  nadie del club los aprobó.
- **Motivo**: la aprobación es un dato uno a uno con el integrante; una tabla aparte no aporta
  nada (§19). No es un historial de estados (§18): solo se guarda la aprobación.
- **Alternativas**: tabla `AprobacionIngreso` (una tabla y un repositorio más para una relación
  uno a uno).

## 11. Rechazo

- **Decisión**: `POST /api/clubes/{clubId}/ingresos/{usuarioRolId}/rechazo`. En una transacción:
  1. Borra el integrante con una sentencia condicionada a `EstadoIngreso = 'EN_ESPERA'`. Si no
     borra nada: `409 ingreso_ya_aprobado` si existe y está aprobado (RF-027), `404` si no existe.
  2. Borra las invitaciones del club (las que no son de presidente) enviadas al correo de esa
     cuenta, usadas o no (RF-027a: no queda rastro de ese correo en la lista).
  3. Si la cuenta se quedó sin ningún integrante, borra la cuenta. Con ella se van, por cascada,
     su foto de perfil y sus solicitudes de recuperación.
- **Motivo**: §12.1.1 y §14 lo definen como eliminación física justificada. Al no quedar nada, el
  correo y el documento quedan libres y la persona puede volver con una invitación nueva
  (RF-027b).
- **Sin aviso** (aclaración de la spec): no se envía ningún correo. Una cuenta borrada recibe
  `401` en su siguiente petición y, al intentar entrar, el `credenciales_invalidas` de siempre.
  Quien conserva otros clubes recibe `404` en ese club y la pantalla lo lleva a uno de los suyos.
- **Reutilización**: la regla "una cuenta que pierde su último integrante se elimina" ya existe en
  el retiro de presidente (001). Se extrae a un colaborador compartido en lugar de copiarla.
- **Confirmación**: la pide la pantalla con `DialogoConfirmacion` (ya existe), avisando de que el
  registro se borrará. El endpoint no necesita un cuerpo de confirmación: no es irreversible para
  el club, que puede volver a invitar.

## 12. Frontend

- **Apartado "Ingresos"**: una pantalla, `/club/:clubId/ingresos`, con tres secciones: sala de
  espera, invitaciones e ingresos aprobados. El enlace del menú solo se muestra al PRESIDENTE y a
  los DIRECTIVOS; la protección real es la del servidor (§15).
- **Sala de espera de la persona**: `DisposicionClub` mira `estadoIngreso` del club en la sesión.
  Si es `EN_ESPERA`, muestra solo la pantalla de espera con el nombre y la identidad del club, sin
  menú, y conserva el desplegable de clubes, el botón de tema y cerrar sesión (RF-019). Un botón
  "Actualizar" recarga la sesión; si ya fue aprobada, entra a la aplicación (RF-024).
- **Qué club se abre al entrar**: sin un último club recordado, se prefiere un club en el que la
  persona esté aprobada (escenario 2.8). Es un ajuste de `ultimoClub.ts`, con su prueba en Vitest.
- **Teléfono** (RF-031, CE-009): las listas usan el componente `Tabla` existente, que ya se adapta
  a 360 px; los datos de cada persona en espera se muestran como ficha apilada.

## 13. Pruebas

Las mismas herramientas de la 001. Lo nuevo que hay que cubrir (§20):

- **Unitarias**: `ReglaAprobacionIngreso`, `ReglaMayoriaDeEdad`, `ReglaIngresoPorInvitacion`,
  `ReglaAccesoPorEstado` con el estado de ingreso, `Invitacion.EstadoEn` y `ValidadorRegistro`
  con el responsable.
- **Integración**: quién puede invitar; invitación de un solo uso, vencida y con otro correo;
  registro que queda en espera; cuenta en espera bloqueada en todos los endpoints del club;
  aprobación por rol, idempotente y concurrente; límites del DIRECTIVO; rechazo que borra del club,
  conserva los otros clubes y permite volver solo con invitación nueva; aislamiento por
  identificador entre clubes; el DESARROLLADOR no ve nada de esto; club suspendido y dado de baja.

## Supuestos a confirmar por el propietario

Ninguno bloquea las tareas; si alguno cambia, el cambio es de una consulta o de una validación.

| # | Supuesto | Dónde se usa |
| --- | --- | --- |
| 1 | La lista de invitaciones del club muestra una fila por correo, la más reciente. Una invitación reemplazada por un reenvío no aparece; una cancelada aparece como "cancelada" hasta que se vuelve a invitar a ese correo | Decisión 3 |
| 2 | Una cuenta en espera de un club suspendido ve el aviso de incidencia temporal, no la pantalla de sala de espera | Decisión 4 |
| 3 | "Menor de 18 años el día del registro" se calcula con la fecha UTC del servidor | Decisión 6 |
| 4 | Las invitaciones de presidente a un club dado de baja siguen comportándose como en la 001; solo las del club dejan de servir durante la baja | Decisión 8 |
| 5 | El nombre del responsable admite hasta 160 caracteres y, cuando la persona ya tiene cuenta y acepta la invitación sin registrarse de nuevo, no se le vuelve a pedir | Decisiones 6 y 7 |
| 6 | La lista de ingresos aprobados no incluye a los presidentes que entraron con invitación del DESARROLLADOR | Decisión 10 |
