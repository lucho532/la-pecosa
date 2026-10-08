# Investigación: Categorías del club

**Funcionalidad**: `003-categorias-club` | **Fecha**: 2026-10-08

Este documento resuelve las incógnitas técnicas del plan. Las tecnologías, el aislamiento por
`ClubId`, la sesión y las pruebas son los de la 001
([research.md de la 001](../001-base-multiclub/research.md)); la autorización por roles y la sala
de espera, los de la 002 ([research.md de la 002](../002-ingreso-club/research.md)). No se repiten
aquí. Cada decisión indica qué se eligió, por qué y qué se descartó. Las marcadas con
**(supuesto)** rellenan un detalle que la spec no fija; están reunidas al final para que el
propietario las confirme.

## 1. Dónde vive la categoría del jugador

- **Decisión**: la categoría actual se guarda en el integrante: `UsuarioRol.CategoriaId`, opcional.
  No se crea todavía la entidad `Jugador`.
- **Motivo**: la spec lo fija en sus supuestos ("aquí el jugador es el integrante aprobado con el
  rol JUGADOR y solo gana su categoría") y deja la ficha del jugador a su funcionalidad. Crear
  ahora una tabla `Jugador` con una sola columna útil sería adelantar esa funcionalidad (§19, §25).
- **Consecuencia**: la constitución escribe `Jugador.CategoriaId` (§11.2). El significado se
  cumple (un único registro por persona y una única categoría actual); el nombre de la tabla no.
  Cuando llegue la funcionalidad de jugadores decidirá si la columna se queda en el integrante o
  pasa a la ficha; el cambio sería una migración que mueve una columna. Queda como nota 1 del plan.
- **Alternativas**: crear `Jugador` mínima ahora (una tabla, un repositorio y una relación uno a
  uno que hoy no aportan nada); una tabla intermedia jugador-categoría (permite dos categorías a la
  vez, que RF-013 prohíbe).

## 2. Tablas nuevas

- **Decisión**: cinco tablas, todas con `ClubId` y bajo el filtro global de aislamiento:

  | Tabla | Para qué | Por qué no se puede evitar |
  | --- | --- | --- |
  | `Categorias` | La categoría: club, año, activa | Es la entidad de la funcionalidad |
  | `Equipos` | División con nombre de una categoría | RF-022 |
  | `AsignacionesEntrenadorCategoria` | Qué integrante entrena qué categoría | §8 y §12.3 la nombran; relación de muchos a muchos con estado (RF-018, RF-021) |
  | `EntrenadoresEquipo` | Qué equipos dirige una asignación | Una asignación dirige varios equipos o ninguno (RF-028) |
  | `JugadoresEquipo` | En qué equipos juega un jugador | Un jugador está en varios equipos a la vez (RF-025) |

- **Motivo**: son las relaciones que pide la spec; ninguna guarda historial (§11.2, §11.3, §18).
- **Alternativas**: guardar los equipos de un jugador o de un entrenador como lista en una columna
  (sin integridad referencial y sin poder contar ni filtrar); una sola tabla "miembro de equipo"
  para jugadores y entrenadores (mezcla dos reglas distintas: el jugador debe ser de la categoría y
  el entrenador debe estar asignado a ella).

## 3. "Nunca ha tenido jugadores ni entrenadores"

- **Decisión**: `Categoria.Usada` y `Equipo.Usado`, booleanos que empiezan en falso y pasan a
  verdadero, para siempre, la primera vez que entra un jugador o un entrenador, sea a mano o por la
  ubicación automática. Solo se borra físicamente con el valor en falso (RF-004a, RF-024a).
- **Motivo**: no se guarda historial de las categorías ni de los equipos por los que pasó un
  jugador, así que "alguna vez tuvo" no se puede deducir de las filas actuales. Un booleano es lo
  mínimo que responde la pregunta y no es un historial (§18).
- **Consecuencia** (caso límite de la spec): si al crear la categoría entran solos jugadores de ese
  año, queda usada en ese mismo instante y ya no se puede borrar.
- **Alternativas**: deducirlo de las asignaciones, que no se borran (no cubre a los jugadores);
  tablas de historial (la spec las deja fuera de alcance).

## 4. Ubicación automática y concurrencia

- **Decisión**: un colaborador de Aplicación, `UbicadorDeJugadores`, con dos operaciones que usan
  todos los puntos de entrada:
  - *Ubicar a uno*: pone al jugador en la categoría activa de su año de nacimiento, si existe. Lo
    usan la aprobación de un ingreso como JUGADOR (RF-008) y la reincorporación (RF-045).
  - *Recoger a los de un año*: pone en una categoría a todos los jugadores aprobados, activos y sin
    categoría nacidos ese año, y devuelve cuántos entraron. Lo usan crear y reactivar (RF-010).

  Las dos son una sola sentencia `UPDATE ... WHERE CategoriaId IS NULL`, de modo que nunca mueven a
  quien ya tiene categoría (RF-011), y marcan la categoría como usada si entró alguien.
- **Concurrencia**: toda operación que ubica jugadores o cambia el estado de una categoría se
  ejecuta en una transacción que primero bloquea la fila del club (`SELECT ... FOR UPDATE`, el
  mismo recurso que ya usa el retiro de presidente de la 001). Así se ejecutan una detrás de otra
  dentro de un club y se cumplen los casos límite:
  - Aprobar a un jugador mientras se crea la categoría de su año: quien llegue segundo ve el
    resultado del primero, y el jugador termina en la categoría.
  - Dos presidentes pasan al mismo jugador a categorías distintas: vale la última.
  - Desactivar una categoría mientras entra un jugador: o entra antes y la desactivación se niega,
    o la categoría ya está inactiva y el jugador queda sin categoría.
- **Dos presidentes crean el mismo año a la vez**: además del bloqueo, un índice único
  `(ClubId, Anio)` decide; el segundo recibe `409`.
- **Año de nacimiento**: se compara `FechaNacimiento` con el intervalo del 1 de enero al 31 de
  diciembre del año, sin columna calculada.
- **Motivo**: un club tiene decenas de jugadores (§19); serializar por club no cuesta nada y evita
  razonar caso por caso.
- **Alternativas**: aislamiento serializable con reintentos (más código y errores transitorios);
  una tarea periódica que recoloque (el jugador quedaría mal ubicado un rato).

## 5. Qué cambia en la aprobación de la 002

- **Decisión**: `ServicioAprobacionIngreso` pasa a ejecutar, en una transacción con el club
  bloqueado, la sentencia condicionada de aprobación que ya tenía y, si el rol es JUGADOR,
  "ubicar a uno". Si el club no tiene activa la categoría del año, el ingreso se aprueba igual
  (RF-009). El contrato de la aprobación no cambia.
- **Motivo**: es la parte de categorías de §12.1.1. La de mensualidades sigue pendiente.
- **Jugadores aprobados antes de esta funcionalidad**: tienen `CategoriaId` nulo, así que ya están
  en "Sin categoría" y entran al crearse la categoría de su año. La migración no mueve datos.

## 6. Categorías: crear, desactivar, reactivar y borrar

- **Año válido** (RF-002): entero entre 1000 y el año en curso. El año en curso es el de la fecha
  UTC del servidor, la misma que ya usa el registro **(supuesto 1)**. Vive en
  `Dominio/Reglas/ReglaAnioDeCategoria`.
- **Ya existe** (RF-003): `409 categoria_ya_existe` si está activa y
  `409 categoria_inactiva_ya_existe` si está inactiva, para que la pantalla ofrezca reactivarla.
- **Desactivar**: se niega con `409 categoria_con_jugadores` si le queda alguno (RF-005). Si pasa,
  en la misma transacción desactiva sus asignaciones y borra lo que dirigían (RF-006). Sus equipos
  no cambian de estado: dejan de mostrarse porque la categoría no se muestra a entrenadores ni a
  familias, y vuelven vacíos al reactivarla porque no quedan jugadores ni entrenadores en ellos.
- **Reactivar**: la deja activa, sin entrenadores, y recoge a los jugadores sin categoría de su año.
- **Borrar**: solo con `Usada` en falso; sus equipos se borran por cascada. Si no,
  `409 categoria_con_historial`.
- **Confirmaciones**: las pide la pantalla con el `DialogoConfirmacion` existente, como en la 002.
  Ninguna de estas operaciones es irreversible para el club salvo el borrado, que solo aplica a lo
  que nunca se usó.

## 7. Asignación de entrenadores

- **Decisión**: una fila por pareja integrante y categoría, con índice único
  `(CategoriaId, UsuarioRolId)` y la columna `Activa`. Asignar es "crear la fila o volver a
  activarla"; retirar es "desactivarla y borrar los equipos que dirigía". Las dos son idempotentes
  (RF-020) y la fila nunca se borra (RF-021) **(supuesto 2: al reasignar se reutiliza la fila)**.
- **A quién se puede asignar** (RF-017): integrante del club, aprobado, con rol ENTRENADOR,
  DIRECTIVO o PRESIDENTE. Vive en `Dominio/Reglas/ReglaEntrenadorAsignable` y se comprueba en el
  servidor contra la fila del integrante. Como la consulta va con el filtro del club, un integrante
  de otro club no existe: `404`.
- **El rol no cambia** (RF-017a): asignar no toca `UsuarioRol.Rol`. No hay nada que conservar ni
  restaurar, y la autorización sigue mirando solo el rol.
- **Equipos que dirige** (RF-028): se reemplaza la lista completa con un `PUT`. Todos los equipos
  deben ser activos y de esa categoría, y la asignación debe estar activa.
- **Alternativas**: una fila nueva por cada vez que se asigna (acumula filas y obliga a buscar
  "la activa"); una columna `EquipoId` en la asignación (solo permite un equipo).

## 8. Equipos

- **Nombre** (RF-023): obligatorio, sin espacios sobrantes, 30 caracteres como máximo. Se guarda
  además normalizado en minúsculas, y un índice único parcial `(CategoriaId, NombreNormalizado)`
  sobre los equipos activos impide repetirlo. El mismo índice resuelve dos altas simultáneas.
- **Nombre de un equipo desactivado** **(supuesto 3)**: queda libre. La spec no prevé reactivar un
  equipo y un equipo desactivado "deja de mostrarse"; si su nombre siguiera ocupado, el PRESIDENTE
  no podría volver a tener un equipo "A" ni ver por qué.
- **Desactivar** (RF-030): marca `Activo = false` y borra sus filas de jugadores y de entrenadores.
  Los jugadores siguen en la categoría y los entrenadores siguen asignados a ella. Un equipo
  inactivo no aparece en ninguna respuesta.
- **Jugadores de un equipo** (RF-025): poner y sacar son operaciones idempotentes sobre la pareja
  jugador y equipo. Se comprueba en el servidor que el jugador está en la categoría del equipo; si
  no, `409 jugador_de_otra_categoria`.
- **Al cambiar de categoría, retirarse o dejar de ser jugador**: se borran sus filas de
  `JugadoresEquipo` en la misma transacción (RF-027, RF-042).

## 9. Quién ve qué

- **Decisión**: el alcance se resuelve en una regla de dominio, `ReglaAlcanceDeCategorias`, a
  partir del rol, que es lo único que decide (§12.3):

  | Rol | Categorías que ve | Listas "Sin categoría" y "Retirados" |
  | --- | --- | --- |
  | PRESIDENTE | Todas, activas e inactivas | Sí |
  | DIRECTIVO | Todas, activas e inactivas | Sí |
  | ENTRENADOR | Solo las activas con una asignación activa suya | No |
  | JUGADOR | Ninguna por estos endpoints | No |

  Un PRESIDENTE o DIRECTIVO asignado como entrenador sigue viendo lo de su rol (RF-017a).
- **ENTRENADOR y una categoría que no es suya**: `404`, igual que si no existiera. No distingue
  "existe pero no es tuya" **(supuesto 4)**. Es el mismo criterio que la constitución usa para lo
  que una cuenta no debe saber que existe (§17.2).
- **La familia** (RF-035): un endpoint propio, `GET /api/clubes/{clubId}/mi-categoria`, solo para
  el rol JUGADOR. Devuelve un DTO distinto, sin identificadores de otras personas: el año de la
  categoría, los nombres de sus equipos y, de cada entrenador, nombre, apellidos y nombres de los
  equipos que dirige. No hay forma de pedir otra categoría: no recibe ningún identificador.
- **Listas de jugadores** (RF-032): nombre, apellidos, año de nacimiento, equipos y la marca
  "fuera de su año" (RF-016), que se calcula comparando el año de nacimiento con el de la
  categoría. Nunca el documento, el correo ni el celular.
- **Mutaciones**: todos los endpoints que cambian algo llevan `[IntegranteDelClub(Rol.PRESIDENTE)]`
  (RF-033, RF-036). El DESARROLLADOR no tiene integrante en ningún club: `404` (RF-038).
- **Club suspendido y dado de baja** (RF-039): lo resuelve el atributo de la 001 sin código nuevo.

## 10. Retiro y reincorporación

- **Decisión**: el retiro se expresa con `UsuarioRol.Activo` (§14.1 lo ordena así), más tres
  columnas con quién retiró y cuándo: `RetiradoEn`, `RetiradoPorUsuarioId` y `RetiradoPorNombre`
  (copia del nombre, §13, igual que la aprobación de la 002).
- **Retirar**: una sentencia condicionada a que sea JUGADOR, aprobado y activo. Pone
  `Activo = false`, `CategoriaId = null` y los datos del retiro, y borra sus equipos. Si no cambia
  ninguna fila: ya estaba retirado, y responde `204` sin efecto (RF-047); o no es un jugador
  aprobado, y responde `409 no_es_jugador` (RF-041).
- **Reincorporar**: `Activo = true`, limpia los datos del retiro y lo ubica como a un ingreso
  recién aprobado. No recupera equipos (RF-045). Los datos del retiro anterior no se conservan:
  solo se guarda el estado actual (§18) **(supuesto 5)**.
- **Acceso del retirado** (RF-043): se impone en el mismo lugar que la sala de espera.
  `ReglaAccesoPorEstado` recibe también `Activo` y devuelve un resultado nuevo,
  `IntegranteRetirado`; `IntegranteDelClubAttribute` lo traduce a `403 integrante_retirado` en
  **todos** los endpoints del club, presentes y futuros. Como el atributo consulta la base de datos
  en cada petición, el retiro se aplica a una sesión ya abierta.
- **Orden de las comprobaciones**: estado del club, después el ingreso y después el retiro. Un
  jugador retirado de un club suspendido ve el aviso de incidencia temporal **(supuesto 6)**.
- **El aviso**: `GET /api/sesion` añade `retirado` a cada club. Con él, el frontend muestra la
  pantalla "Ya no estás en este club" con el nombre y la identidad que ya trae la sesión, sin
  pedir nada al club. Es la misma excepción de §7.1 que usa la sala de espera.
- **Correo y documento ocupados** (RF-046): la fila del integrante se conserva, así que los
  índices únicos siguen impidiendo repetirlos. Lo que cambia es el mensaje:
  - Invitar a ese correo desde el club: `409 persona_retirada`.
  - Registrarse en ese club con ese documento, o aceptar una invitación antigua con esa cuenta:
    `409 persona_retirada`.
- **Lista de retirados** (RF-044): integrantes con `Activo = false`, del retiro más reciente al más
  antiguo.
- **Alternativas**: un valor `RETIRADO` en `EstadoIngreso` (§14.1 dice expresamente que no es un
  estado de ingreso); una tabla `Retiro` (relación uno a uno con el integrante, §19).

## 11. Otros puntos de la 001 y la 002 que se ven afectados

- **Invitación de presidente a quien ya es jugador del club** (001): la aceptación le reemplaza el
  rol por PRESIDENTE. Desde ahora, en esa misma transacción sale de su categoría y de sus equipos y
  queda activo, porque solo los jugadores tienen categoría (RF-012) y el retiro solo aplica a
  jugadores.
- **Quitar el rol a un presidente** (001): si pasa a DIRECTIVO o ENTRENADOR, conserva sus
  asignaciones, porque los tres roles son asignables. Si se le elimina del club, sus asignaciones
  se borran con él por cascada; es la eliminación física que ya decidió la 001. La categoría sigue
  marcada como usada **(supuesto 7)**.
- **Rechazo de un ingreso** (002): quien está en espera no tiene categoría, equipos ni
  asignaciones; no cambia.
- **Ingresos aprobados** (002): es el registro de las aprobaciones; un jugador retirado sigue
  apareciendo en él **(supuesto 8)**.
- **Eliminar un club** (001): las cinco tablas nuevas cuelgan del club con borrado en cascada.

## 12. Frontend

- **Apartado "Categorías"**: `/club/:clubId/categorias`, visible en el menú para PRESIDENTE,
  DIRECTIVO y ENTRENADOR. Lista las categorías por año con su número de jugadores, sus equipos y
  sus entrenadores. Para PRESIDENTE y DIRECTIVO añade "Sin categoría" y "Retirados". Las acciones
  solo se pintan al PRESIDENTE; la protección real es la del servidor (§15).
- **Detalle de una categoría**: `/club/:clubId/categorias/:categoriaId`, con tres secciones:
  entrenadores, equipos y jugadores. En la sección de jugadores, cada fila muestra una casilla por
  equipo, de modo que repartir 20 jugadores entre dos equipos son 20 toques.
- **Familia**: una tarjeta "Mi categoría" en el inicio del club, solo para el rol JUGADOR.
- **Retirado**: `DisposicionClub` mira `retirado` en la sesión antes de pedir nada al club, igual
  que hace con la sala de espera, y `ultimoClub.ts` deja de preferir un club en el que la persona
  está retirada.
- **Teléfono y temas** (RF-040, CE-015): componentes `Tabla`, `Tarjeta`, `EtiquetaEstado` y
  `DialogoConfirmacion` existentes. "Inactiva" y "fuera de su año" se indican con texto, no solo
  con color (§24).

## 13. Pruebas

Las mismas herramientas de la 001. Lo nuevo que hay que cubrir (§20, versión 3.8.0):

- **Unitarias**: `ReglaAnioDeCategoria`, `ReglaEntrenadorAsignable`, `ReglaAlcanceDeCategorias`,
  `ReglaAccesoPorEstado` con el retiro y la normalización del nombre de equipo.
- **Integración**: solo el PRESIDENTE cambia algo, recorriendo todos los endpoints de escritura con
  cada rol; ubicación automática al aprobar, al crear y al reactivar; un jugador nunca en dos
  categorías ni en un equipo de otra; PRESIDENTE y DIRECTIVO asignados conservan rol y alcance;
  el entrenador no ve una categoría ajena y ve todos los equipos de las suyas; la familia ve solo
  su categoría, sus equipos y el nombre de sus entrenadores; borrado solo de lo nunca usado; el
  retirado recibe `403` en todos los endpoints del club y conserva sus datos; aislamiento por
  identificador entre clubes; club suspendido y dado de baja; concurrencia de crear la misma
  categoría y de aprobar mientras se crea.
- **Prueba de contrato**: `AccesoClubPruebas` compara los endpoints de la API con los contratos de
  todas las specs y hoy espera 33. Con este contrato pasan a 55; la prueba se actualiza en la
  primera tarea y solo vuelve a estar en verde cuando existan los 22 endpoints nuevos.

## Supuestos confirmados por el propietario

Confirmados el 2026-10-08 y recogidos en la spec (sección "Supuestos", RF-042 y RF-046). El mismo
día confirmó que quien acepta una invitación de PRESIDENTE siendo jugador retirado de ese club
queda como PRESIDENTE activo (decisión 11, RF-046).

| # | Supuesto | Dónde se usa |
| --- | --- | --- |
| 1 | "El año en curso" para validar el año de una categoría es el de la fecha UTC del servidor. Solo cambia el resultado durante las cinco horas de diferencia con Colombia del 31 de diciembre | Decisión 6 |
| 2 | Volver a asignar a un entrenador que ya estuvo en esa categoría reactiva la misma asignación; no se crea otra | Decisión 7 |
| 3 | El nombre de un equipo desactivado queda libre: se puede crear otro equipo con ese nombre en la misma categoría. Un equipo desactivado no se puede reactivar ni se muestra | Decisión 8 |
| 4 | Un ENTRENADOR que pide una categoría que no tiene asignada recibe `404`, igual que si no existiera | Decisión 9 |
| 5 | Al reincorporar a un jugador no se conserva quién lo retiró ni cuándo | Decisión 10 |
| 6 | Un jugador retirado de un club suspendido ve el aviso de incidencia temporal, no el de retiro | Decisión 10 |
| 7 | Si el DESARROLLADOR elimina del club a un presidente que entrenaba, sus asignaciones se borran con él; la categoría sigue contando como usada y no se puede borrar | Decisión 11 |
| 8 | La lista "Ingresos aprobados" de la 002 sigue mostrando a un jugador retirado | Decisión 11 |
