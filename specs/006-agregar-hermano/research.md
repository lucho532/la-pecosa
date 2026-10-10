# Investigación: Agregar un hermano y elegir el jugador

**Funcionalidad**: `006-agregar-hermano` | **Fecha**: 2026-10-09

Este documento resuelve las incógnitas técnicas del plan. Las tecnologías, el aislamiento por
`ClubId`, la sesión y las pruebas son los de la 001
([research.md de la 001](../001-base-multiclub/research.md)); la sala de espera, la aprobación y
el rechazo, los de la 002 y la 004 ([research.md de la 004](../004-invitacion-con-rol/research.md));
la ubicación automática y el bloqueo del club, los de la 003
([research.md de la 003](../003-categorias-club/research.md)); la ficha y su regla de acceso, los
de la 005 ([research.md de la 005](../005-ficha-jugador/research.md)). No se repiten aquí. Cada
decisión indica qué se eligió, por qué y qué se descartó. Las marcadas con **(supuesto)** rellenan
un detalle que la spec no fija; están reunidas al final para que el propietario las confirme.

## 1. Qué es un hermano en el modelo

- **Decisión**: un hermano es **otra fila de `UsuarioRol`** con el mismo `UsuarioId` y el mismo
  `ClubId` que el primer hijo, rol JUGADOR y `EstadoIngreso = EN_ESPERA`. No hay tabla nueva.
- **Motivo**: desde la 003 el jugador **es** el `UsuarioRol` con rol JUGADOR: ahí viven sus
  nombres, su documento, su fecha de nacimiento, su categoría, sus equipos, su retiro y su estado
  de ingreso, y de él cuelgan `FichaJugador` y `DocumentoJugador` (005). Una segunda fila da al
  hermano todo eso sin tocar nada: ficha propia (RF-017), categoría propia, retiro propio. El
  correo, la contraseña, el celular y el responsable están en `Usuario`, así que se comparten sin
  copiarlos (RF-003).
- **Lo que ya lo permite**: `UsuariosRol` no tiene índice único por cuenta y club (el índice
  `ClubId, UsuarioId` no es único); el único índice único es `ClubId, NumeroDocumento`, que es
  justo RF-005.
- **Lo que hoy lo impide**: todo el código supone un integrante por cuenta y club.
  `RepositorioPertenencias.ObtenerAsync(usuarioId, clubId)` devuelve el más antiguo y
  `IntegranteDelClubAttribute` lo usa como "quien pregunta" en cada petición. Las secciones 2 a 4
  cambian esa suposición en un solo sitio.
- **Alternativas**: crear ahora la entidad `Jugador` de §12.3 (la misma reescritura de la 003, la
  004 y la 005 que ya se descartó en la 005); una cuenta nueva por hermano con el mismo correo
  (lo prohíbe §12.1: el correo es único).

## 2. Cómo sabe el servidor con cuál jugador continúa la familia

- **Decisión**: la petición lo dice con una cabecera, `X-Jugador-Elegido`, que lleva el
  `UsuarioRolId` del jugador elegido. `IntegranteDelClubAttribute` deja de pedir "el integrante de
  la cuenta en el club" y pide **todos**; una regla de dominio nueva,
  `ReglaJugadorDeLaSesion`, decide cuál es el de la petición:

  | Integrantes de la cuenta en el club | Cabecera | Resultado |
  | --- | --- | --- |
  | Ninguno | — | `404`, como hoy |
  | Uno | Ausente, o ese mismo | Ese integrante, como hoy |
  | Uno | Otro identificador | `404` |
  | Varios | Uno de ellos | Ese integrante |
  | Varios | Ausente | `409 jugador_sin_elegir` |
  | Varios | Otro identificador | `404` |

  Después se aplica al elegido lo de siempre: estado del club, ingreso en espera, retiro y rol.
- **Motivo**: todos los servicios reciben ya "quien pregunta" como un `UsuarioRol`
  (`ControladorBase.Integrante`). Si ese objeto es el jugador elegido, "Mi categoría", "Mi ficha",
  `ClubDto.MiUsuarioRolId` y todo lo que venga después (pagos, convocatorias) quedan atados al
  jugador elegido sin cambiar una línea de ellos. El hermano en espera recibe
  `403 ingreso_en_espera` y el retirado `403 integrante_retirado` por el mecanismo que ya existe
  (RF-013). Una cuenta con un solo integrante no envía nada y se comporta exactamente como antes,
  así que las pruebas de la 001 a la 005 no cambian.
- **Por qué una cabecera y no el token**: cambiar de jugador (RF-025) sería pedir un token nuevo y
  guardar la elección dentro de él, que es precisamente lo que RF-027 no quiere recordar. La
  cabecera no da ningún permiso que la cuenta no tenga: solo vale si el identificador es de un
  integrante **de esa cuenta en ese club** (RF-029).
- **Por qué no en la ruta**: las rutas de la ficha ya llevan `{usuarioRolId}`, pero "Mi
  categoría", el inicio del club y las futuras pantallas del jugador no; habría que duplicar todas.
- **CORS**: la política del frontend debe admitir la cabecera nueva.

## 3. Entrar con el documento: una sesión limitada a ese jugador

- **Decisión**: cuando el identificador del inicio de sesión es un documento, el token lleva una
  reclamación nueva, `jugadores`, con los `UsuarioRolId` de la cuenta que tenían ese número en ese
  momento (uno por club: el documento es único por club). `ValidadorSesion` la deja disponible en
  la petición y `ReglaJugadorDeLaSesion` la aplica **antes** que la cabecera: en un club donde la
  cuenta tiene varios integrantes, el de la petición es el que está en la reclamación, la cabecera
  no puede cambiarlo y, si ninguno está en ella, la respuesta es `404`. Renovar el token conserva
  la reclamación.
- **Motivo**: RF-026 y RF-031 piden que la limitación sea del servidor y dure toda la sesión. El
  token es lo único que identifica la sesión (no hay sesiones guardadas), así que la limitación
  tiene que viajar en él.
- **Identificadores y no el número**: la familia puede cambiar el documento del jugador durante
  la sesión (005, historia 4). Con el número en el token la sesión se rompería al guardarlo.
- **Clubes con un solo integrante**: la reclamación no cambia nada en ellos. Quien entra con su
  documento y pertenece a otro club con otro rol sigue viendo ese club, como en la 001.
- **Entrar con el correo** no añade la reclamación: la sesión llega a todos los jugadores de la
  cuenta.
- **Alternativas**: dos tokens distintos por tipo de sesión (misma información, más código);
  resolver por número de documento en cada petición (se rompe al cambiar el documento).

## 4. Qué devuelve la sesión

- **Decisión**: `GET /api/sesion` sigue devolviendo **una entrada por club**. `ClubDeSesionDto`
  gana `usuarioRolId` y `jugadores`, la lista de los integrantes de la cuenta en ese club
  (identificador, nombres, apellidos, estado de ingreso, retirado). `jugadores` solo trae
  elementos cuando hay algo que elegir: la cuenta tiene más de un integrante en el club **y** la
  sesión no está limitada. Los campos que ya existían (`rol`, `estadoIngreso`, `retirado`,
  `nombres`, `apellidos`) describen al único integrante, al de la reclamación o, si hay que
  elegir, al más antiguo.
- **Motivo**: hoy la sesión devuelve una entrada por integrante y la aplicación busca el club con
  `clubes.find(clubId)`; con dos hermanos habría dos entradas del mismo club y el desplegable de
  clubes lo mostraría dos veces. Agrupar por club mantiene el desplegable y las pruebas de la 001,
  y la lista vacía es la señal de "no hay nada que elegir" (RF-023, RF-026).
- **Una sesión limitada no recibe los hermanos**: ni nombres ni estados (RF-026: "ni recibe datos
  de sus hermanos").

## 5. Dónde vive la elección en la aplicación

- **Decisión (supuesto)**: en la memoria de la pestaña, respaldada en `sessionStorage` por club,
  y se borra al iniciar y al cerrar sesión. `cliente.ts` añade la cabecera a las peticiones de
  `/api/clubes/{clubId}/…` cuando hay una elección para ese club.
- **Motivo**: RF-027 pide elegir de nuevo cada vez que se entra con el correo; recargar la página
  no es volver a entrar, y pedir la elección en cada recarga sería molesto. `sessionStorage` muere
  con la pestaña; el token, que sí se recuerda, no guarda la elección.
- **Pantalla**: no hay ruta nueva. `DisposicionClub` muestra la lista de jugadores cuando la
  sesión trae `jugadores` y no hay elección válida, antes de pedir nada del club (RF-021). Al
  elegir o cambiar, la aplicación del club se monta de nuevo con el jugador como clave, para que
  no quede en pantalla ningún dato del anterior (CE-009), y vuelve al inicio del club.
- **Si la elección deja de valer** (rechazaron o borraron al jugador): el servidor responde `404`
  o `409 jugador_sin_elegir`; la aplicación recarga la sesión, descarta la elección y vuelve a la
  lista, o entra directamente si queda uno solo.

## 6. Agregar al hermano

- **Decisión**: un endpoint nuevo, `POST /api/clubes/{clubId}/jugadores/{usuarioRolId}/hermanos`,
  solo para el rol JUGADOR. `{usuarioRolId}` debe ser el jugador de la petición (el elegido); si
  no, `404`. Crea el `UsuarioRol` en espera dentro de una transacción con el club bloqueado, el
  mecanismo de la 003.
- **Desde quién se puede** (RF-001, RF-008): el atributo ya niega la petición a un jugador en
  espera o retirado, porque el de la petición es el elegido. No hace falta código propio.
- **Validación**: nombres, apellidos, documento y fecha de nacimiento con `ValidadorIdentidad`,
  el mismo del registro y de la ficha (RF-006).
- **Responsable** (RF-004): si el hermano es menor según `ReglaMayoriaDeEdad` y la cuenta no
  tiene responsable, el cuerpo debe traer `nombreResponsable` y se guarda en `Usuario`. Si la
  cuenta ya lo tiene, el campo se ignora: cambiar el responsable es cosa de la ficha (005).
- **Documento** (RF-005): `409` si ya lo tiene otro integrante del club, activo, retirado o en
  espera; lo garantiza el índice único `ClubId, NumeroDocumento`, que no distingue estados.
  Si lo usa **otra cuenta** en otro club, lo decide la sección 12 (RF-034 y RF-039), que
  sustituye al supuesto con el que se construyó: `409` en todos los casos. Si es de la misma
  cuenta en otro club se admite: es el mismo niño en dos clubes (§10).
- **Confirmar dos veces** (RF-009): **(supuesto)** si el número ya es de un jugador **en espera
  de la misma cuenta** en ese club, la respuesta es `200` con ese jugador, sin crear ni cambiar
  nada. El bloqueo del club hace que la segunda petición vea siempre a la primera.
- **Sin límite** (RF-010): no se cuenta nada.
- **Sin correo** (supuesto de la spec): el servicio no usa `IServicioCorreo`.

## 7. De quién es hermano

- **Decisión**: una columna nueva y opcional en `UsuariosRol`, `AgregadoDesdeUsuarioRolId`, que
  apunta al jugador desde cuya ficha se agregó, con borrado a `NULL`. `IngresoEnEsperaDto` gana
  `hermanoDe`, con los nombres y apellidos de ese jugador.
- **Motivo**: RF-015 pide que el PRESIDENTE vea de qué jugador es hermano. Deducirlo de "otro
  integrante de la misma cuenta" falla cuando la cuenta ya tiene tres hijos; la spec habla del
  jugador de origen, que es un dato y no una deducción.
- **No se copia el nombre**: solo se muestra mientras el hermano está en espera; no es histórico
  (§13). Si el jugador de origen deja de existir, `hermanoDe` llega vacío.

## 8. Aprobar y rechazar

- **Aprobar**: no cambia. `ServicioAprobacionIngreso` ya deja al integrante APROBADO con rol
  JUGADOR y lo ubica con `UbicadorDeJugadores` (RF-016). La ficha nace vacía porque no tiene fila
  hasta el primer cambio (005, research §1; RF-017).
- **Rechazar** tiene un cambio obligado: hoy, tras borrar al integrante, borra las invitaciones
  del club a ese correo. Con un hermano eso borraría la invitación usada del primer hijo, que es
  el registro de cómo entró (004, RF-019). Las invitaciones solo se borran cuando a la cuenta no
  le queda **ningún** integrante en el club. `EliminadorDeCuentaSinClub` no necesita cambios: la
  cuenta sigue teniendo al primer hijo (RF-018).
- **Volver a agregarlo** (RF-019): el rechazo borra la fila, así que el número queda libre.

## 9. La ficha entre hermanos

- **Decisión**: en `AccesoAFicha`, "es de su cuenta" pasa a ser "es el jugador de la petición":
  `jugador.Id == quienPregunta.Id`. `ReglaAccesoAFicha` no cambia de forma; se renombra el
  parámetro.
- **Motivo**: RF-030. Hoy la comparación es por cuenta (`UsuarioId`), con lo que la familia, con
  un hijo elegido, podría leer la ficha del otro cambiando el identificador de la ruta. La spec
  pide un único jugador en cada momento, y una sesión limitada por documento no debe llegar al
  hermano por ningún camino.
- **Cambia un requisito de la 005** (RF-005 de la 005), como ya anota la spec.

## 10. Lo que sigue suponiendo un integrante por cuenta y club

Revisado todo lo que usa `IRepositorioPertenencias` o compara por `UsuarioId`:

| Sitio | Qué hace hoy | Con hermanos |
| --- | --- | --- |
| `ServicioAceptacionInvitacion` | Una invitación del club a quien ya está en él se rechaza | Igual: basta con que exista uno |
| `ServicioAceptacionInvitacion`, invitación de PRESIDENTE | Convierte en PRESIDENTE al integrante existente | Convierte al más antiguo; los demás siguen como JUGADOR. Ver "Consecuencias" en el plan |
| `EstadoDeIngresoDelCorreoAsync` (invitar a un correo) | Mira al más antiguo | Igual: el más antiguo siempre está aprobado, porque solo un aprobado agrega hermanos |
| `EliminadorDeCuentaSinClub`, borrado de club | Cuentan integrantes | Sin cambios |
| `MapperFicha` (¿el último cambio lo hizo la familia?) | Compara por cuenta | Correcto: la familia es la cuenta |
| Listas de jugadores, candidatos a entrenador | Filtran `APROBADO` | El hermano en espera no aparece (RF-012) |
| RF-039 de la 005 (celular y responsable) | Son de la cuenta | Valen para todos los hermanos sin código |

## 11. Pruebas

- **Unitarias**: `ReglaJugadorDeLaSesion` (la tabla de la sección 2 y la reclamación de la 3) y
  el validador del hermano.
- **Integración** (§20), en `Integracion/Hermanos/`: agregar y sus rechazos; el hermano en
  espera; la elección por cabecera; la sesión con documento; aprobar; rechazar; aislamiento entre
  hermanos y entre cuentas; dos clubes; roles que no pueden agregar; estado del club.
- **No regresión**: las pruebas de la 001 a la 005 no envían cabecera y usan cuentas con un solo
  integrante, así que deben seguir en verde sin tocarlas, salvo las dos que nombra el plan.
- **Contrato**: `AccesoClubPruebas` pasa de 60 a 61 endpoints.

## Incremento del 2026-10-09: el documento que ya usa otra cuenta (RF-034 a RF-040)

Las secciones 12 a 17 son solo de este incremento. Las secciones 1 a 11 describen lo ya
construido y siguen vigentes, salvo el supuesto 1.

## 12. Cuándo se admite el documento de otra cuenta

- **Decisión**: una regla de dominio nueva, `ReglaDocumentoCompartido`, recibe los integrantes que
  tienen ese número, en cualquier club, y la cuenta que agrega. Mira solo los de **otra cuenta**:

  | Integrantes de otra cuenta con ese número | Resultado | Respuesta al agregar |
  | --- | --- | --- |
  | Ninguno | `SinBaja` | `201`, `retiraDeOtroClub: false` |
  | Alguno que no es JUGADOR | `NoAdmitido` | `409 documento_en_otra_cuenta` (RF-039) |
  | Solo jugadores, y alguno aprobado y activo | `ConBaja` | `201`, `retiraDeOtroClub: true` (RF-034) |
  | Solo jugadores, todos retirados o en espera | `SinBaja` | `201`, `retiraDeOtroClub: false` |

  La comprobación del propio club va antes y no cambia: `409 documento_repetido_en_club`.
- **Motivo**: las tres preguntas del incremento (¿se admite?, ¿aprobarlo retira a alguien?, ¿qué
  cuenta abre el documento?) dependen de los mismos pocos integrantes. Una regla sin HTTP ni EF se
  prueba fila por fila y deja los repositorios sin reglas (§5).
- **El aviso a la familia** (RF-034): el endpoint devuelve `HermanoAgregadoDto`, que son los cinco
  campos de `JugadorDeSesionDto` más `retiraDeOtroClub`. No se añade el campo a
  `JugadorDeSesionDto`, que también viaja en la sesión, donde no tiene sentido. **(supuesto 10)**
  El aviso llega después de crear, como dice la historia 1.11.
- **Confirmar dos veces**: el `200` que devuelve al hermano que ya esperaba calcula el aviso en
  ese momento.
- **Alternativas**: una consulta previa "¿este documento retira a alguien?" antes de confirmar
  (un endpoint más y un oráculo sin coste para quien pregunta); un código `2xx` distinto (la
  pantalla tendría que tratar tres respuestas de éxito).

## 13. La baja al aprobar

- **Decisión**: `ServicioAprobacionIngreso`, dentro de su transacción:
  1. Lee el ingreso y los integrantes con su número.
  2. Si la regla da `NoAdmitido`, responde `409 documento_en_otra_cuenta` **(supuesto 7)**.
  3. Bloquea los clubes: solo el suyo si no hay nadie que retirar, como hoy; el suyo y los de los
     jugadores que va a retirar, si los hay.
  4. Aprueba con la sentencia condicionada de siempre y ubica al jugador.
  5. Retira, con una sentencia condicionada, a todo jugador aprobado y activo de otra cuenta con
     ese número, en cualquier club: lo saca de sus equipos, `Activo = false`, sin categoría,
     `RetiradoEn` con la hora de la aprobación.
- **Una sola transacción** (RF-036): si la baja falla, la aprobación se deshace con ella.
- **El paso 5 se ejecuta siempre**, haya visto o no a alguien en el paso 1: la spec pide decidir
  con lo que haya "en el momento de aprobar". Sin documento compartido no cambia ninguna fila.
- **Por qué se bloquea el otro club**: el retiro de la 003 lo hace con su club bloqueado para que
  una asignación a un equipo que ocurra a la vez no deje a un retirado dentro de él. La baja
  automática necesita la misma garantía.
- **Por qué en orden**: es la única operación que bloquea más de un club. Si el club A aprueba a
  alguien activo en B mientras B aprueba a alguien activo en A, cada uno esperaría al otro. Los
  clubes se bloquean en una sola sentencia, ordenados por identificador, y así no hay abrazo
  mortal. Por eso la lectura va antes del bloqueo y no después, como hoy.
- **Quién lo retiró** **(supuesto 6)**: `RetiradoPorUsuarioId` y `RetiradoPorNombre` quedan
  vacíos. El nombre de quien aprueba es un dato de este club y no debe aparecer en la lista de
  retirados del otro (§7.1); RF-037 tampoco quiere avisos.
- **Lo que no toca** (RF-037): la ficha, los documentos, el historial, la cuenta anterior, su
  contraseña, sus demás jugadores, ni a quien espera con ese número en otro club. Tampoco al
  jugador de la **misma** cuenta en otro club: es el mismo niño inscrito en los dos.
- **Dónde vive lo que cruza clubes**: `IRepositorioRetiroEntreClubes`, con dos operaciones
  (bloquear clubes en orden y retirar por número y cuenta). Es el único que escribe fuera del club
  de la petición, y queda separado de `IRepositorioPertenencias` para que esa excepción a §7.1 se
  vea y se pruebe sola.
- **Alternativas**: reutilizar `ServicioRetiroJugador` (trabaja con el filtro del club de la
  petición y con quien retira; habría que cambiar de club a mitad de la transacción); no bloquear
  el otro club (deja la carrera con los equipos); bloquear primero el propio y luego los demás
  (abrazo mortal entre dos aprobaciones cruzadas).

## 14. El aviso al PRESIDENTE

- **Decisión**: `IngresoEnEsperaDto` gana `retiraDeOtroClub`. `ServicioConsultaIngresos` lee, en
  una consulta, los integrantes que tienen los números de quienes esperan y aplica la regla a cada
  uno: `true` solo con `ConBaja`.
- **Se calcula al abrir** la sala de espera, no al agregar: si el otro club lo retiró o lo
  reincorporó entre tanto, el aviso lo refleja (caso límite de la spec). No se guarda.
- **Qué no lleva** (RF-038): ni el nombre ni el identificador del otro club, ni el correo, el
  nombre o el identificador de la otra cuenta, ni cuántos clubes son. Es un booleano.
- **Pantalla**: el aviso se pinta en el ingreso y se repite en el diálogo de aprobar, que es donde
  se decide.

## 15. Qué cuenta abre un documento

- **Decisión**: `ServicioSesion` deja de pedir "la cuenta de ese número" y pide los integrantes
  que lo tienen; `ReglaDocumentoCompartido` elige uno y se abre su cuenta (RF-040):
  1. El aprobado y activo.
  2. Si no hay ninguno, el que está en espera.
  3. Si tampoco, el retirado.

  **(supuesto 8)** Entre dos del mismo nivel, el más reciente.
- **Con la contraseña de otra cuenta**: se compara con la de la cuenta elegida y no coincide. La
  respuesta es el `401 credenciales_invalidas` de siempre, con el mismo texto y el mismo cálculo
  de hash; no se prueba contra las demás cuentas.
- **La limitación del token** no cambia: los integrantes de la cuenta elegida con ese número.
- **Con el documento en una sola cuenta** el resultado es el de hoy.
- **Consecuencia**: los fallos se cuentan en la cuenta elegida. La familia anterior, entrando con
  el documento y su contraseña, puede bloquear la cuenta nueva al quinto intento.

## 16. Lo que sigue sin aceptar un documento de otra cuenta

| Sitio | Hoy | Con el documento en dos cuentas |
| --- | --- | --- |
| Registro con invitación (004) | `409` si el número tiene dueño | Igual: basta con que exista uno. Sin cambios de código |
| Cambio de documento desde la ficha (005) | `409` si el dueño es otra cuenta | "El dueño" deja de ser uno solo. Pasa a preguntar si lo tiene **alguna** otra cuenta: mismo `409` en los mismos casos, y deja de depender de qué fila devuelva la consulta |
| Reincorporar a un retirado (003) | No mira el documento | **(supuesto 9)** No cambia. Si el otro club reincorpora al que salió por esta vía, el número queda activo en dos cuentas y decide el desempate de la sección 15 |

## 17. Pruebas del incremento

- **Unitarias**: `ReglaDocumentoCompartido`: la tabla de la sección 12 fila por fila, a quién
  retira y el orden de la sección 15 con sus desempates.
- **Integración**, en `Integracion/Hermanos/`:
  - `DocumentoDeOtraCuentaPruebas`: historias 1.11 y 1.12; retirado o en espera en el otro club;
    la misma cuenta en otro club; mientras espera, el otro club no cambia (RF-035).
  - `BajaEnOtroClubPruebas`: historias 3.9 a 3.12; el aviso no trae nada del otro club; varios
    clubes; ya retirado allí; cambios en el otro club entre agregar y aprobar; dos familias de
    clubes distintos con el mismo número en espera; dos aprobaciones cruzadas a la vez; el otro
    PRESIDENTE lo ve en sus retirados sin autor; ningún correo.
  - `SesionConDocumentoCompartidoPruebas`: historias 2.12 y 2.13; la contraseña de la otra cuenta
    recibe la misma respuesta que una incorrecta; el token queda limitado al jugador de la cuenta
    elegida; la familia anterior entra con el correo y ve el aviso de retiro.
- **Cambian**: en `AgregarHermanoRechazosPruebas`, la que esperaba `409` para el documento de otra
  cuenta. Se añade un caso a `CambioDeDocumentoPruebas` y otro a `RegistroConInvitacionPruebas`.
- **Contrato**: `AccesoClubPruebas` sigue en 61 endpoints.

## Supuestos por confirmar

1. No se admite el documento de un hermano que ya usa **otra cuenta** en otro club (sección 6).
   La spec dice que un integrante de otro club "se puede agregar aquí"; este plan lo cumple solo
   cuando es de la misma cuenta.
   **Cambiado por el propietario el 2026-10-09**: se admite y, al aprobarse, el jugador queda
   retirado del otro club (spec, RF-034 a RF-040). Lo resuelven las secciones 12 a 17.
2. Confirmar dos veces el mismo hermano devuelve el que ya está en espera, sin error (sección 6).
3. La elección sobrevive a recargar la página y se pierde al cerrar la pestaña, al cerrar sesión
   y al volver a entrar (sección 5).
4. Si la cuenta ya tiene responsable, el que se escriba al agregar un hermano se ignora
   (sección 6).
5. Si el jugador de origen deja de existir, la sala de espera no dice de quién es hermano
   (sección 7).
6. La baja automática no guarda quién retiró; el otro club lo ve en sus retirados sin autor
   (sección 13).
7. Al aprobar se vuelve a aplicar RF-039: si el documento es ya de alguien que no es JUGADOR con
   otra cuenta, `409` y no se aprueba (sección 13).
8. Entre dos cuentas del mismo nivel, el documento abre la del integrante más reciente
   (sección 15).
9. La reincorporación en el otro club no cambia ni mira el documento (sección 16). **Pide
   decisión del propietario**: dejarlo así o negarla mientras el documento esté activo con otra
   cuenta.
10. El aviso a la familia llega en la respuesta de agregar, después de crear (sección 12).
