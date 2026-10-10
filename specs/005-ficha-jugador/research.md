# Investigación: Ficha del jugador

**Funcionalidad**: `005-ficha-jugador` | **Fecha**: 2026-10-09

Este documento resuelve las incógnitas técnicas del plan. Las tecnologías, el aislamiento por
`ClubId`, la sesión, la carga de imágenes y las pruebas son los de la 001
([research.md de la 001](../001-base-multiclub/research.md)); las categorías, el alcance del
entrenador, el retiro, la ubicación automática y el bloqueo del club, los de la 003
([research.md de la 003](../003-categorias-club/research.md)); el registro directo con rol, el de la
004 ([research.md de la 004](../004-invitacion-con-rol/research.md)). No se repiten aquí. Cada
decisión indica qué se eligió, por qué y qué se descartó. Las marcadas con **(supuesto)** rellenan
un detalle que la spec no fija; están reunidas al final para que el propietario las confirme.

## 1. Dónde se guarda la ficha

- **Decisión**: una tabla nueva, `FichasJugador`, con una fila por jugador y club, cuya clave
  primaria es el `UsuarioRolId` del jugador. Guarda solo lo que la ficha **añade**: contacto de
  emergencia, seguridad social, datos clínicos y el último cambio. La identidad (nombres,
  apellidos, documento, fecha de nacimiento, categoría, equipos) sigue en `UsuarioRol`, y el
  correo, el celular y el responsable siguen en `Usuario`, donde ya están desde el registro.
- **Motivo**: la spec define la ficha como "los datos de un jugador en un club" que "reúne la
  identidad y el contacto que ya existen y añade…" (Entidades clave). Separar lo añadido evita que
  las consultas de sesión, de listas y de autorización, que leen `UsuarioRol` en cada petición,
  carguen textos clínicos, y deja los datos más restringidos en una tabla que solo tocan los
  servicios de la ficha.
- **La fila nace con el primer cambio**: una ficha que nadie ha cambiado no tiene fila y se lee
  como una ficha vacía, sin último cambio (escenario 1.10). Así no hay que tocar el registro, la
  aceptación ni la aprobación de la 004, ni rellenar filas para los jugadores que ya existen
  (supuesto de la spec: "quedan con la ficha vacía, sin ningún tratamiento especial").
- **Borrado** (RF-037): la fila cuelga de `UsuarioRol` y del club con borrado en cascada. Rechazar
  un ingreso, eliminar a alguien del club, eliminar una cuenta que se quedó sin club o eliminar el
  club borran la ficha sin código nuevo. Retirar a un jugador no la toca (RF-036): el retiro solo
  cambia `Activo`.
- **Alternativas**: columnas nuevas en `UsuariosRol` (trece columnas que solo usa un rol, cargadas
  en cada petición); crear ahora la entidad `Jugador` de §11.2 y §12.3 y mover a ella la categoría
  y el retiro (reescribe la 003 y la 004 sin que la spec lo pida; ver "Seguimiento de complejidad"
  del plan); crear la fila al registrarse (obliga a cambiar tres servicios y a una migración de
  datos para un resultado que el usuario no distingue).

## 2. Quién ve y quién cambia: una sola regla

- **Decisión**: una regla de dominio, `ReglaAccesoAFicha`, responde con un `AlcanceDeFicha` (cinco
  indicadores) a partir de cuatro datos: el rol de quien pregunta, si el jugador es de su cuenta, si
  el jugador está activo y si quien pregunta tiene una asignación activa en la categoría activa del
  jugador.

  | Quien pregunta | Ve la ficha | Datos clínicos | Documentos | Cambia | Corrige identidad |
  | --- | --- | --- | --- | --- | --- |
  | PRESIDENTE | Cualquier jugador del club: con categoría, sin ella o retirado | Sí | Sí | Sí | Sí |
  | DIRECTIVO | Igual que el PRESIDENTE | **No**, tampoco si entrena la categoría | Sí | No | No |
  | ENTRENADOR | Solo jugadores activos de una categoría activa que tiene asignada | Sí | **No** | No | No |
  | JUGADOR | Solo los jugadores de su cuenta | Sí | Sí | Sí, menos la identidad | No (sí el documento) |

- **"De su cuenta"** se resuelve comparando la cuenta (`UsuarioId`) del jugador con la de quien
  pregunta, no su identificador de integrante. Es lo que escribe §12.3 ("JUGADOR: mediante
  `Jugador.UsuarioId`") y deja resuelto el acceso cuando exista el hermano agregado desde la
  ficha, sin adelantar nada de esa funcionalidad: hoy cada cuenta tiene un solo jugador por club.
- **Qué es "jugador" para la ficha** (RF-004): un integrante con rol JUGADOR e ingreso `APROBADO`,
  activo o retirado. El identificador de un ENTRENADOR, un DIRECTIVO, un PRESIDENTE o alguien en
  espera responde `404`, igual que uno que no existe.
- **Un colaborador compartido**, `AccesoAFicha`, lee al jugador, consulta la asignación del
  entrenador y aplica la regla. Lo usan los cuatro servicios de la ficha, para que ninguna
  operación pueda olvidar la comprobación. Si la regla dice que no ve la ficha lanza `404
  no_encontrado` (RF-012): mismo código y mismo mensaje que para un identificador inexistente.
- **Qué responde cada negativa** (RF-012, RF-019):
  - No puede ver esa ficha (otra cuenta, otra categoría, otro club, retirado para el entrenador):
    `404 no_encontrado`.
  - Su rol nunca puede hacer esa operación (un DIRECTIVO o un ENTRENADOR que cambian algo, un
    ENTRENADOR que abre o sube un archivo, una familia que corrige nombres): `403
    rol_no_autorizado`, que ya pone `[IntegranteDelClub(...)]` antes de leer nada. No revela si el
    jugador existe, porque se responde igual para cualquier identificador.
  - Jugador retirado en su propia cuenta: `403 integrante_retirado`, el del atributo (RF-013).
  - DESARROLLADOR y otro club: `404`, el del atributo (RF-011).
- **Los datos que no se ven no viajan** (RF-010, RF-015): `FichaJugadorDto` lleva los datos
  clínicos y los documentos como grupos opcionales. Para quien no puede verlos el grupo no se
  rellena y la propiedad **no aparece en el JSON** (se omite, no va en `null`), de modo que una
  prueba puede afirmar que la respuesta de un DIRECTIVO no contiene la clave `datosClinicos`. El
  DTO lleva además `permisos`, para que la pantalla no decida por su cuenta qué botones mostrar.
- **El entrenador pierde el acceso al instante** (escenario 2.8 y caso límite): la asignación se
  consulta en cada petición, como ya hace `ServicioConsultaCategorias`.
- **Alternativas**: un DTO distinto por rol (cuatro tipos casi iguales y cuatro mapeos que
  mantener iguales); decidir en cada servicio con condiciones sueltas (la regla central de la
  constitución sobre la ficha quedaría repartida y sin prueba unitaria propia).

## 3. Operaciones de la API

Seis endpoints nuevos, todos bajo `/api/clubes/{clubId}/jugadores/{usuarioRolId}/ficha`:

| Operación | Quién (atributo) | Qué hace |
| --- | --- | --- |
| `GET …/ficha` | Los cuatro roles | La ficha, con lo que el alcance permite |
| `PUT …/ficha` | PRESIDENTE, JUGADOR | Contacto, contacto de emergencia, seguridad social y datos clínicos |
| `PUT …/ficha/documento-identidad` | PRESIDENTE, JUGADOR | Tipo y número de documento |
| `PUT …/ficha/identidad` | PRESIDENTE | Nombres, apellidos y fecha de nacimiento |
| `GET …/ficha/documentos/{documento}` | PRESIDENTE, DIRECTIVO, JUGADOR | Abre el archivo entregado |
| `PUT …/ficha/documentos/{documento}` | PRESIDENTE, JUGADOR | Sube o reemplaza el archivo |

- **Una sola ruta con identificador, también para la familia**: "Mi ficha" es la misma ruta con el
  identificador del propio jugador. Para que la pantalla lo conozca, `ClubDto` (`GET
  /api/clubes/{clubId}`, que ya se pide al entrar al club) gana `miUsuarioRolId`. Con una ruta
  `mi-ficha` aparte habría que duplicar las seis operaciones, y dejaría de servir cuando una cuenta
  tenga varios jugadores.
- **Identidad separada del resto**: nombres, apellidos y fecha de nacimiento tienen su propia
  operación, solo de PRESIDENTE. Así el escenario 1.6 se cumple dos veces: `PUT …/ficha` no tiene
  esos campos (si llegan de más se ignoran, como un `rol` de más en el registro) y `PUT
  …/ficha/identidad` responde `403` a la familia.
- **Guardar reemplaza el formulario entero**: `PUT …/ficha` recibe todos los campos de contacto y
  de salud; un campo vacío o ausente queda vacío. Es lo que hace cierto el caso límite "queda el
  último cambio guardado".
- **El correo no se cambia**: no está en ningún cuerpo (RF-018).
- **Sin borrar archivos**: no hay `DELETE` de un documento; la familia reemplaza, no deja pendiente
  (supuesto de la spec).
- Todas las respuestas de la ficha y de los archivos llevan `Cache-Control: private, no-store`.

## 4. Celular y responsable: datos de la cuenta

- **Decisión** (RF-039): `PUT …/ficha` escribe `Usuario.Celular` y `Usuario.NombreResponsable` de
  la cuenta del jugador. No se copian a la ficha: al ser de la cuenta, el cambio se ve solo en
  todos los jugadores y clubes de esa persona.
- **Validación**: el celular sigue siendo obligatorio, con el máximo del registro (20). El
  responsable es obligatorio si **este** jugador es menor de 18 años hoy (`ReglaMayoriaDeEdad`, la
  de la 004) y opcional si es adulto (RF-020).
- **Aislamiento** (§7.1): `Usuario` no es un dato de club; el PRESIDENTE solo llega a esa cuenta a
  través de un jugador de su club, y solo escribe esos dos campos. Es el único efecto entre clubes
  de la funcionalidad y lo decidió el propietario en la spec.
- **Alternativa descartada**: copiar celular y responsable a cada ficha (contradice RF-039).

## 5. Cambio del documento de identidad

- **Decisión** (RF-022 a RF-024): actualiza `TipoDocumento` y `NumeroDocumento` de la misma fila de
  `UsuarioRol`. El número se normaliza y se valida como en el registro. El inicio de sesión con
  documento ya busca por ese campo, así que entra con el número nuevo sin tocar la sesión; la
  contraseña y las sesiones abiertas no cambian.
- **Documento repetido en el club** (RF-023): `409 documento_repetido_en_club`, el código que ya
  usa el registro, también si el otro integrante está retirado. Se comprueba antes y lo garantiza
  el índice único que ya existe (`IX_UsuariosRol_ClubId_NumeroDocumento`), cuya violación se
  traduce al mismo error cuando dos cambios coinciden.
- **Documento de otra cuenta** **(supuesto 1)**: si el número nuevo ya lo tiene un integrante de
  **otra cuenta** en otro club, se responde `409 documento_en_otra_cuenta`, la misma regla que ya
  aplica el registro. Sin ella, dos cuentas distintas compartirían número y el inicio de sesión
  con documento no sabría a cuál entrar. Si el número es de la misma cuenta en otro club, se
  admite: es la misma persona (§10).
- **Persona en dos clubes**: el cambio se hace en la ficha de un club y solo cambia ese club
  (RF-003; RF-039 dice que el celular y el responsable son el único dato con efecto en otro club).
  Mientras el otro club conserve el número anterior, ese número sigue sirviendo para entrar. Con un
  solo club, el número anterior deja de servir de inmediato (escenario 4.2).
- **Enviar el mismo documento** que ya tiene no es un error y no registra un cambio.

## 6. Corrección de la identidad y categoría

- **Decisión** (RF-025, RF-026): corregir la fecha de nacimiento nunca mueve a quien ya tiene
  categoría. Si el jugador está activo y sin categoría, después de guardar se llama al
  `UbicadorDeJugadores` de la 003, en la misma transacción y con el club bloqueado: entra en la
  categoría activa del año nuevo, o sigue sin categoría si no existe. Un jugador retirado no se
  ubica (no es "jugador del club" para el ubicador).
- **Validación**: nombres y apellidos con las reglas del registro; fecha de nacimiento obligatoria
  y no futura (RF-021). Las comprobaciones de identidad que hoy están dentro de
  `ValidadorRegistro` se extraen a `ValidadorIdentidad`, que usan los dos, para que el registro y
  la ficha no puedan divergir.
- **Lo ya copiado no cambia** (§13): el nombre guardado en una aprobación o un retiro anterior
  (`AprobadoPorNombre`, `RetiradoPorNombre`) es histórico y se queda como estaba.
- **Fecha que convierte en menor a quien no tiene responsable** **(supuesto 2)**: la corrección se
  guarda; el responsable pasa a ser obligatorio la próxima vez que alguien guarde el contacto.
  Bloquear la corrección obligaría al PRESIDENTE a conocer un dato de la familia para arreglar un
  error del club.

## 7. Documentos: qué son y dónde se guardan

- **Lista fija** (RF-027): enumeración `DocumentoPedido` con `COPIA_DOCUMENTO_IDENTIDAD` y
  `CERTIFICADO_SALUD`. No hay tabla de tipos ni configuración por club.
- **Almacenamiento**: tabla `DocumentosJugador` en PostgreSQL, con los bytes en una columna, igual
  que el escudo y la foto de perfil. Clave primaria `(UsuarioRolId, Documento)`: como máximo un
  archivo por documento pedido (RF-029).
- **Motivo**: reemplazar es una sola sentencia dentro de una transacción, así que un fallo a mitad
  de la subida deja el archivo anterior, o deja el documento pendiente si no había (caso límite y
  RF-030); el borrado en cascada cumple RF-037 sin limpiar archivos sueltos; el filtro por club se
  aplica solo; la copia de seguridad es la de la base de datos. A la escala del proyecto (decenas
  de jugadores por club, dos archivos cada uno, §19) el tamaño no es un problema.
- **Tamaño y formatos** (la spec los deja al plan): hasta **10 MB** por archivo; **PDF, JPEG, PNG
  y WebP**, reconocidos por su firma binaria y no por la extensión ni por lo que declara el
  navegador. 10 MB admite la foto de un documento tomada con el teléfono sin obligar a reducirla.
  - Demasiado grande: `400 archivo_demasiado_grande`. Otro formato o sin archivo: `400
    archivo_no_admitido`. En ambos casos no se toca el archivo anterior.
  - El límite de la petición de esta operación sube a 12 MB, para que sea el validador y no el
    servidor quien explique el motivo (mismo criterio que `ArchivoCargado.LimiteDePeticion`).
  - `ValidadorArchivoDeFicha` reutiliza la detección de firmas de `ValidadorImagen`, que se hace
    accesible, y añade la del PDF. `ValidadorImagen` conserva su límite de 1 MB para el escudo y
    la foto.
- **No se guarda el nombre del archivo** que envía la familia: no hace falta y evita sanear un
  dato ajeno. Al abrirlo se sirve con un nombre fijo según el documento y el formato.
- **Las listas nunca leen los bytes**: el estado (entregado, fecha, formato) se consulta con una
  proyección que excluye la columna del contenido.
- **Abrir un archivo** (RF-031): `GET` con sesión; la respuesta lleva el tipo de contenido
  detectado al subir, `X-Content-Type-Options: nosniff`, `Content-Disposition: inline` y
  `Cache-Control: private, no-store`. La pantalla lo pide con `api.getArchivo` y lo abre desde
  memoria en otra pestaña, como la foto de perfil: nunca hay una dirección pública.
- **El parámetro `{documento}` no lleva restricción de ruta**: así la autorización se ejecuta antes
  de leerlo y las pruebas genéricas de acceso de la 001, que rellenan todos los parámetros con un
  identificador, siguen recibiendo `401` y `404`. Un valor que no es de la lista responde `404`.
- **Alternativas**: archivos en un volumen de disco o en un almacenamiento de objetos (tecnología
  y despliegue nuevos, archivos huérfanos, reemplazo no atómico; §6.3 y §19 piden no introducir
  tecnologías sin necesidad); 5 MB (rechaza fotos normales de teléfonos recientes); reducir la
  imagen en el navegador (complejidad que nadie pidió).

## 8. Estado de la documentación en las listas

- **Decisión** (RF-032): `JugadorDeCategoriaDto` y `JugadorRetiradoDto` ganan
  `documentosPendientes`, un entero de 0 a 2. La pantalla muestra "Completa" o "Faltan N" con
  texto, no solo con color.
- **El ENTRENADOR no lo recibe** (RF-007): el detalle de una categoría también lo ve él, así que
  para un ENTRENADOR la propiedad se omite del JSON, igual que los grupos de la ficha. Las listas
  "Sin categoría" y "Retirados" ya son solo de PRESIDENTE y DIRECTIVO.
- **Cómo se calcula**: una consulta que cuenta los documentos entregados de los jugadores de la
  lista, agrupada por jugador; no una consulta por fila. `documentosPendientes` es 2 menos ese
  número. No se guarda ningún contador (CE-009: siempre coincide con los archivos reales).
- **Sin avisos** (RF-002 y fuera de alcance): no hay correos, contadores en el menú ni filtros.

## 9. Último cambio

- **Decisión** (RF-038): `FichasJugador` guarda `UltimoCambioEn`, `UltimoCambioPorUsuarioId` y
  `UltimoCambioPorNombre`. Las cinco operaciones que cambian algo (contacto y salud, documento de
  identidad, identidad, subir archivo, y la corrección de fecha que además ubica) sellan esos tres
  datos en la misma transacción, creando la fila si no existe. No se guarda historial ni valores
  anteriores (§18).
- **El nombre** es el del integrante que actúa, copiado en ese momento (§13), como ya hacen la
  aprobación y el retiro. Cuando actúa la cuenta del jugador, ese nombre es el del propio jugador,
  porque la familia no tiene un integrante aparte; el DTO lleva `porLaCuentaDelJugador` para que
  la pantalla escriba "la cuenta del jugador" en lugar de repetir su nombre **(supuesto 3)**.
- **Guardar sin cambiar nada** no sella un cambio: la fecha solo se mueve si algún dato quedó
  distinto.
- **Celular o responsable cambiados desde otra ficha** **(supuesto 4)**: el sello se pone en la
  ficha desde la que se hizo el cambio. La ficha de ese jugador en otro club, o la de un hermano,
  muestra el dato nuevo pero conserva su propio último cambio.
- **Borrar la cuenta de quien cambió** deja el nombre copiado y vacía la referencia, como en la
  aprobación y el retiro.

## 10. Concurrencia

- **Decisión**: las cinco operaciones de cambio se ejecutan en una transacción con la fila del
  club bloqueada (`IRepositorioClub.BloquearAsync`, el mecanismo de la 003). Dos guardados casi
  simultáneos sobre la misma ficha se ejecutan uno tras otro: queda el último y ningún dato se
  mezcla (caso límite). El mismo bloqueo resuelve la creación simultánea de la fila de la ficha y
  la corrección de fecha mientras el PRESIDENTE crea la categoría de ese año.
- **Motivo**: es el patrón que ya usa todo cambio dentro de un club; a esta escala serializar los
  cambios de fichas de un club no se nota. El archivo ya está leído en memoria antes de abrir la
  transacción, así que el bloqueo no espera a la red.
- **Alternativa**: control optimista con número de versión (obliga a la pantalla a resolver
  conflictos; la spec pide expresamente "queda el último cambio guardado").

## 11. Frontend

- **Ruta nueva**: `/club/:clubId/jugadores/:usuarioRolId/ficha`, una sola pantalla para todos los
  roles. Muestra y permite lo que dicen los grupos presentes y `permisos` del DTO.
- **Entradas** (RF-033, RF-034):
  - JUGADOR: enlace "Mi ficha" en el menú del club y en el inicio, con `miUsuarioRolId`. Los demás
    roles no lo ven (no tienen ficha).
  - PRESIDENTE, DIRECTIVO y ENTRENADOR: el nombre de cada jugador en la lista de una categoría es
    un enlace a su ficha. PRESIDENTE y DIRECTIVO, también en "Sin categoría" y "Retirados".
- **Secciones**: identidad (solo lectura; el PRESIDENTE la corrige en un diálogo y el documento se
  cambia en otro), contacto, contacto de emergencia, seguridad social, datos clínicos y
  documentos. Contacto y salud son un solo formulario con un botón "Guardar". Quien no puede
  cambiar ve los mismos datos como texto. Las secciones que el DTO no trae no se pintan.
- **Documentos**: por cada documento pedido, su estado con texto ("Entregado el …" o "Pendiente"),
  "Abrir" y, si puede cambiar, "Subir" o "Reemplazar". La pantalla comprueba tamaño y extensión
  antes de enviar solo para avisar pronto; quien decide es la API.
- **Último cambio**: una línea al pie de la cabecera; no aparece si nadie ha cambiado la ficha.
- **Tipos**: en un archivo nuevo `tiposFicha.ts`; `tipos.ts` tiene 241 líneas y no admite más
  (§2.3).
- **Teléfono y temas** (RF-035): los componentes existentes (`Tarjeta`, `Campo`, `Tabla`,
  `EtiquetaEstado`, `DialogoConfirmacion`, `Aviso`); los campos largos son áreas de texto.
- **Retirado**: `DisposicionClub` ya muestra solo el aviso y no pinta el menú (escenario 1.8).

## 12. Pruebas

Las mismas herramientas de la 001. Seis endpoints nuevos: la prueba de contrato pasa de 54 a 60.

- **Unitarias**: `ReglaAccesoAFicha` (la tabla de la decisión 2, caso por caso, incluido el
  DIRECTIVO y el PRESIDENTE asignados como entrenadores); `ValidadorFicha` (todo opcional,
  longitudes, celular, responsable según la edad); `ValidadorIdentidad`; `ValidadorArchivoDeFicha`
  (cada firma, PDF, tamaño, vacío, una extensión falsa).
- **Integración**, por historia:
  - **Familia**: lee su ficha; guarda y vuelve a leer; todo vacío es válido; responsable
    obligatorio en un menor y opcional en un adulto; nombres enviados de más se ignoran y `PUT
    identidad` responde `403`; la ficha de otro jugador responde `404` al leer, al guardar y al
    cambiar el documento; el retirado recibe `403 integrante_retirado`; el último cambio aparece
    tras cambiar y no antes.
  - **Club**: el PRESIDENTE ve todo de un jugador con categoría, sin ella y retirado; el
    ENTRENADOR ve la ficha de su categoría sin la clave `documentos`, y recibe `404` en otra
    categoría, sin categoría, retirado y tras perder la asignación o cambiar el jugador de
    categoría; el DIRECTIVO ve todo sin la clave `datosClinicos`, también asignado como
    entrenador; DIRECTIVO y ENTRENADOR reciben `403` en las cuatro operaciones de cambio; otro
    club y el DESARROLLADOR reciben `404`.
  - **Documentos**: subir, abrir, reemplazar; formato no admitido y tamaño excedido conservan el
    anterior; el ENTRENADOR recibe `403` al abrir y al subir; otra cuenta de jugador recibe `404`;
    el estado en las tres listas coincide con lo subido; el ENTRENADOR no recibe
    `documentosPendientes`.
  - **Identidad**: cambio de documento conservando categoría, equipos y ficha; entra con el número
    nuevo y no con el anterior; repetido en el club, activo o retirado, `409`; de otra cuenta,
    `409`; corrección de nombres; fecha con categoría (no se mueve), sin categoría (se ubica o
    sigue sin ella), futura (`400`); el PRESIDENTE cambia contacto y salud y sube un archivo.
  - **Entre clubes y cuentas** (RF-003, RF-039): una persona en dos clubes tiene dos fichas
    independientes; el celular cambiado en una se lee en la otra.
  - **Conservación** (RF-036, RF-037): retirar y reincorporar conserva ficha y archivos; rechazar
    un ingreso y eliminar el club no dejan filas en `FichasJugador` ni en `DocumentosJugador`.
  - **Transversal**: club suspendido (solo el PRESIDENTE), dado de baja, y las pruebas genéricas
    de `AccesoClubPruebas` sobre los seis endpoints nuevos.
- **Frontend** (Vitest): la función que convierte `documentosPendientes` en el texto de la lista.

## 13. Lo que queda para otras funcionalidades

- **Hermano agregado desde la ficha** (§12.1.2): la regla de acceso ya compara por cuenta, pero la
  pantalla para agregarlo, la elección de jugador al entrar y la ficha de quien está en espera no
  se construyen aquí.
- **Cambio de rol** (§12.2): cuando exista, pasar una cuenta de JUGADOR a ENTRENADOR o DIRECTIVO
  tendrá que borrar su fila de `FichasJugador` y sus `DocumentosJugador`. La decisión pendiente
  "Ficha con historial" de §28 sigue abierta; este plan no la toca.
- **Eliminación de la cuenta a petición** (§12.4): al borrar a los integrantes de la cuenta, el
  borrado en cascada ya se lleva sus fichas y archivos.

## Supuestos por confirmar

Rellenan detalles que la spec no fija. Ninguno inventa una regla de negocio nueva: cada uno aplica
al caso no previsto la regla más cercana de la spec, de la constitución o de lo ya construido
(§25).

| # | Supuesto | Dónde se usa | Si no se confirma |
| --- | --- | --- | --- |
| 1 | No se admite poner a un jugador un número de documento que ya usa otra cuenta en otro club (`409 documento_en_otra_cuenta`), como ya hace el registro | Decisión 5 | Quitar la comprobación; el inicio de sesión con documento podría entrar a la cuenta equivocada |
| 2 | Corregir la fecha de nacimiento no exige responsable aunque el jugador pase a ser menor; se exigirá al guardar el contacto | Decisión 6 | Añadir la validación a `PUT …/ficha/identidad` |
| 3 | Cuando el último cambio lo hizo la familia, la ficha dice "la cuenta del jugador" en lugar de un nombre de persona | Decisión 9 | Mostrar el nombre del responsable guardado en la cuenta |
| 4 | El último cambio se registra solo en la ficha desde la que se hizo, aunque el celular o el responsable cambien también en otras fichas de la cuenta | Decisión 9 | Sellar todas las fichas de la cuenta, en todos sus clubes |
| 5 | Cada archivo admite hasta 10 MB, en PDF, JPEG, PNG o WebP | Decisión 7 | Cambiar una constante y el texto de ayuda |
