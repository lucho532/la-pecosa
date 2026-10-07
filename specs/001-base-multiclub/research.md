# Investigación: Base multiclub y panel de administración de la plataforma

**Funcionalidad**: `001-base-multiclub` | **Fecha**: 2026-10-07

Este documento resuelve las incógnitas técnicas del plan. Cada decisión indica qué se eligió, por
qué y qué alternativas se descartaron. Las decisiones marcadas con **(supuesto)** rellenan un
detalle que la spec no fija; están reunidas al final para que el propietario las confirme.

## 1. Versión de .NET

- **Decisión**: .NET 10 (LTS), con ASP.NET Core 10 y Entity Framework Core 10.
- **Motivo**: .NET 9 deja de tener soporte en noviembre de 2026, un mes después de empezar. .NET 10
  tiene soporte hasta noviembre de 2028.
- **Alternativas**: .NET 9, descartado por el fin de soporte.
- **Estado**: confirmado por el propietario el 2026-10-07.

## 2. Aislamiento entre clubes

- **Decisión**: una sola base de datos y un solo esquema. Toda tabla que pertenece a un club lleva
  la columna `ClubId` y su entidad implementa `IPerteneceAClub`. El contexto de Entity Framework
  aplica un filtro global de consulta por `ClubId` a todas esas entidades, tomando el club de
  `IContextoClub`. Si no hay club en el contexto, el filtro no devuelve nada (falla cerrado).
- **Motivo**: cumple §7.1 ("ninguna consulta puede ejecutarse sin estar limitada a un club") sin
  depender de que cada consulta recuerde filtrar. Es lo más sencillo para clubes pequeños (§19).
- **Excepciones controladas** (§7.1): solo los repositorios de `Repositorios/Plataforma/` pueden
  saltarse el filtro, y solo en tres casos: el panel del DESARROLLADOR (clubes, presidentes e
  invitaciones, lo que §8 le permite); las consultas de la propia cuenta
  (`RepositorioPertenencias`, siempre por `UsuarioId` o por documento); y abrir una invitación por
  el hash de su token (`RepositorioInvitacionesPorToken`).
- **El escudo no es una excepción**: el endpoint público fija en `IContextoClub` el club de la
  ruta y lee con el filtro activo.
- **Red de seguridad**: una prueba de integración recorre el modelo de EF y falla si alguna
  entidad no implementa `IPerteneceAClub` y no está en la lista expresa de entidades de la
  plataforma (`Club`, `Usuario`, `FotoPerfil`, `SolicitudRecuperacion`). Así, las funcionalidades futuras no
  pueden añadir una tabla sin aislar.
- **Alternativas**: una base de datos o un esquema por club (demasiada operación para decenas de
  jugadores por club); seguridad a nivel de fila de PostgreSQL (potente, pero reparte las reglas
  entre código y base de datos y complica las migraciones).

## 3. Club elegido y estado del club en cada petición

- **Decisión**: el club va en la ruta: `/api/clubes/{clubId}/...`. Un filtro de autorización
  comprueba **en cada petición**, contra la base de datos, que el usuario tiene un integrante en
  ese club y cuál es el estado del club. El token de sesión no guarda ni el club ni el rol.
- **Motivo**: cumple RF-030 (el estado del club se aplica a sesiones ya abiertas) y RF-019a (quien
  es eliminado del club deja de entrar de inmediato) sin mecanismos de revocación. Con el tamaño
  previsto, una consulta por petición no es un problema. La ruta explícita hace que el frontend
  pueda enlazar directamente a un club y que el desplegable sea solo navegación.
- **Respuestas**: sin integrante en ese club → `404` (no se revela que el club existe); club
  suspendido y rol distinto de PRESIDENTE → `403` con código `club_suspendido`; club dado de baja
  → `403` con código `club_dado_de_baja`.
- **Alternativas**: club y rol dentro del token (quedan desactualizados hasta que caduca);
  cabecera `X-Club-Id` (menos visible y fácil de olvidar).

## 4. Sesión

- **Decisión**: un único token JWT firmado, enviado en la cabecera `Authorization: Bearer`. Lleva
  el identificador del usuario y su `SelloSeguridad`. En cada petición se carga el usuario y se
  compara el sello; cambiar la contraseña genera un sello nuevo y todos los tokens anteriores
  dejan de servir. Vigencia de 7 días, con `POST /api/sesion/renovacion` para obtener uno nuevo
  mientras el actual sea válido **(supuesto)**. El frontend lo guarda en el almacenamiento local.
- **Motivo**: §6 fija JWT. Un solo mecanismo, sin tabla de sesiones. La cabecera funciona igual en
  la web y dentro de Capacitor (§6.1), donde las cookies entre orígenes distintos dan problemas.
- **Alternativas**: token corto más token de renovación en cookie `httpOnly` (más resistente a un
  robo por XSS, pero añade una tabla, rotación y manejo de cookies en el WebView de Android);
  ASP.NET Core Identity completo (su modelo de roles globales no encaja con un rol por club y
  obligaría a nombres en inglés).

## 5. Contraseñas, bloqueo y recuperación

- **Decisión**:
  - Hash con `PasswordHasher` de `Microsoft.AspNetCore.Identity` (PBKDF2), detrás de la interfaz
    `IHashContrasena`. No se usa el resto de Identity.
  - Contraseña válida: entre 8 y 128 caracteres, sin reglas de composición **(supuesto)**.
  - Contador de intentos fallidos en `Usuario`, incrementado de forma atómica en la base de datos.
    Al quinto fallo, `Bloqueada = true`. Un acierto antes del quinto lo reinicia.
  - Cualquier fallo de inicio de sesión (cuenta inexistente, contraseña incorrecta, cuenta
    bloqueada) devuelve el mismo `401` con el mismo texto, que incluye cómo recuperar la
    contraseña. Así se cumplen a la vez el escenario 2.11 y la parte de RF-005 que prohíbe revelar
    si una cuenta existe. Cuando la cuenta no existe se calcula igualmente un hash, para que el
    tiempo de respuesta no la delate.
  - Recuperación: tabla `SolicitudRecuperacion` con el hash del token, un solo uso y caducidad de
    60 minutos **(supuesto)**. La respuesta es siempre `202`, exista o no el correo.
  - Límite de peticiones por IP en los endpoints anónimos de cuenta, con el limitador incluido en
    ASP.NET Core.
  - Normalización (decidido por el propietario el 2026-10-07, RF-002): correo y documento se
    guardan y se buscan sin espacios en los extremos y en minúsculas; el documento, además, sin
    espacios ni puntos. Se hace en un único lugar, `NormalizadorTexto`. La contraseña nunca se
    normaliza: recortarla o pasarla a minúsculas la debilitaría.
- **Alternativas**: tokens firmados sin tabla (obligan a persistir el anillo de claves de Data
  Protection, una complejidad menos visible que una tabla); bloqueo temporal (la aclaración de la
  spec pide bloqueo hasta recuperar por correo).

## 6. Cuenta DESARROLLADOR

- **Decisión**: `Usuario.EsDesarrollador` con un índice único parcial que impide que haya más de
  uno (RF-001). Al arrancar, si no existe ninguno, la API crea la cuenta con el correo de la
  configuración `Plataforma:CorreoDesarrollador` y **sin contraseña**. El DESARROLLADOR crea la
  suya con "olvidé mi contraseña". No hay ningún endpoint que cree o modifique esa marca.
- **Motivo**: respeta §12.4 ("nadie asigna la contraseña de otra persona") y evita guardar una
  contraseña inicial en la configuración. El DESARROLLADOR no tiene integrante en ningún club, de
  modo que cualquier endpoint de club le responde `404` (RF-004).
- **Su correo no admite invitaciones** (decidido por el propietario el 2026-10-07, RF-001): crear
  un club, invitar a un presidente o corregir una invitación con el correo de la cuenta
  DESARROLLADOR responde `409` con el código `correo_del_desarrollador` y no crea nada. Así esa
  cuenta nunca llega a tener un integrante.
- **Alternativas**: contraseña inicial por variable de entorno (la constitución 3.0.0 eliminó las
  contraseñas por defecto); un `UsuarioRol` sin club (obliga a un `ClubId` nulo en la tabla de
  pertenencia y debilita el filtro de aislamiento).

## 7. Modelo de cuenta e integrante

- **Decisión**: dos entidades.
  - `Usuario`: la cuenta. Correo único, contraseña, celular.
  - `UsuarioRol`: la pertenencia a un club (el "integrante" de la spec). Lleva `ClubId`, `UsuarioId`, el rol único y los datos
    de identidad (nombres, apellidos, tipo y número de documento, fecha de nacimiento). El
    documento es único por pareja documento + club (§10).
- **Motivo**: §8 dice que cada jugador tiene "su propio registro de integrante, con su propio
  documento" y que una cuenta puede tener varios jugadores. Por eso el documento y el nombre viven
  en el integrante y no en la cuenta. Dejarlo así desde ahora evita mover columnas cuando llegue
  la funcionalidad de jugadores.
- **Nombre**: la entidad se llama `UsuarioRol`, como fija §12.3 (confirmado por el propietario
  el 2026-10-07). Es lo que la spec llama "integrante": además del rol guarda la identidad de la
  persona en ese club.
- **Persona única**: una persona es única en la plataforma y tiene un único inicio de sesión
  (confirmado por el propietario el 2026-10-07). Por eso un mismo número de documento no puede
  estar en dos cuentas distintas. Quien intenta registrarse con un documento que ya pertenece a
  otra cuenta recibe un mensaje que le pide entrar con esa cuenta.
- **Persona que ya tiene cuenta (RF-018)**: al aceptar la invitación con sesión iniciada, el nuevo
  integrante copia los datos de identidad del integrante que ya tiene. En esta funcionalidad toda
  cuenta representa a una sola persona; el caso de una cuenta con varios jugadores se resuelve en
  la funcionalidad de jugadores.
- **Alternativas**: una tercera tabla `Persona` entre cuenta e integrante (normaliza mejor, pero
  es una tabla y un concepto que la constitución no nombra; se puede introducir más adelante si la
  funcionalidad de jugadores lo pide).

## 8. Invitaciones

- **Decisión**: tabla `Invitacion` con club, rol, correo, hash SHA-256 del token, vencimiento a
  los 7 días, fecha de uso y fecha de anulación. El token son 32 bytes aleatorios y solo viaja en
  el enlace del correo. El enlace lleva el token en el fragmento (`/invitacion#<token>`) para que
  no quede en los registros del servidor web; el frontend lo envía a la API en el cuerpo.
- **Reenviar o corregir el correo**: anula la invitación anterior y crea otra (RF-012). Crear una
  invitación para un club y un correo que ya tienen una vigente anula la anterior.
- **Fallo del correo**: el club y la invitación se guardan igualmente; la invitación queda con
  `EstadoEnvio = FALLIDO` y el panel ofrece reenviarla (supuesto de la spec). El envío se intenta
  después de confirmar la transacción, sin colas.
- **Alternativas**: guardar el token en claro (una fuga de la base de datos permitiría
  registrarse como presidente); cola de correos (innecesaria a este tamaño).

## 9. Correo

- **Decisión**: `IServicioCorreo` en Aplicacion. `ServicioCorreoBrevo` en Infraestructura llama
  con `HttpClient` al API transaccional de Brevo, sin su SDK. En desarrollo, si no hay llave de
  Brevo, se usa `ServicioCorreoRegistro`, que escribe el correo en el registro de la API.
- **Motivo**: §6.3. Una llamada HTTP no justifica una dependencia. La implementación de
  desarrollo permite probar todo el flujo sin cuenta de Brevo.

## 10. Escudo

- **Decisión**: el escudo se guarda en PostgreSQL, en la tabla `EscudoClub` (una fila por club),
  separada de `Club` para no cargar los bytes en cada consulta. Formatos PNG, JPEG y WebP,
  comprobados por su firma binaria; máximo 1 MB (confirmado por el propietario). No se admite SVG, porque puede
  contener código. Se sirve en `GET /api/publico/clubes/{clubId}/escudo`, sin sesión, con una
  versión en la dirección para poder cachearlo.
- **Motivo**: al estar en la base de datos, eliminar un club borra su escudo en la misma
  operación (§7.4) y la copia de seguridad es una sola. Es anónimo porque una etiqueta de imagen
  no envía la cabecera de sesión, porque la pantalla de registro lo necesita antes de que exista
  cuenta y porque el escudo es identidad pública del club (§9).
- **Alternativas**: archivos en un volumen de Docker (hay que borrarlos y respaldarlos aparte);
  almacenamiento de objetos externo (una tecnología nueva sin necesidad).

## 11. Colores, temas y contraste

- **Decisión**:
  - El club guarda dos colores en formato `#RRGGBB`: principal y acento (supuesto de la spec). Sin
    colores, se usa la identidad neutra de la plataforma.
  - Los temas se hacen con variables CSS y el atributo `data-tema` en `<html>`. La elección se
    guarda en el almacenamiento local del dispositivo; sin elección, manda
    `prefers-color-scheme`. Un script mínimo en `index.html` aplica el tema antes de pintar.
  - Los colores del club se usan solo como relleno (cabecera, botones principales, distintivos).
    El color del texto sobre ellos se elige automáticamente entre claro y oscuro según el
    contraste. El texto sobre el fondo del tema usa siempre la paleta de la plataforma. Con eso el
    texto se lee en los dos temas por construcción (RF-034).
  - Aviso de RF-009: antes de guardar, el panel calcula el contraste WCAG de cada color contra el
    fondo del tema claro y contra el del oscuro. Si alguno queda por debajo de 3:1, avisa
    indicando en qué tema se perdería el color. Es un aviso, no un bloqueo.
- **Dónde se calcula**: en el frontend (`contraste.ts`), con pruebas en Vitest. No es una barrera
  de seguridad ni de integridad, así que §15 no exige repetirlo en el servidor; el servidor sí
  valida el formato de los colores.
- **Alternativas**: Tailwind o una librería de componentes (no hacen falta para dos temas y dos
  colores variables); calcular el contraste en el servidor (un viaje de red por cada color
  probado).

## 12. Eliminación de un club

- **Decisión**: toda tabla de club tiene clave foránea a `Club` con borrado en cascada. Eliminar
  es una transacción: se comprueba que el club está dado de baja y que el nombre de confirmación
  coincide, se borra el club (la cascada borra integrantes, invitaciones y escudo) y se borran las
  cuentas que se quedaron sin ningún integrante. La cuenta DESARROLLADOR nunca se toca.
- **Motivo**: §7.4 y RF-029. La cascada garantiza que no queda rastro aunque funcionalidades
  futuras añadan tablas; la prueba del modelo de la decisión 2 comprueba también que toda entidad
  de club tiene esa cascada.

## 13. Validación, mapeo y errores

- **Decisión**: validadores y mappers escritos a mano en `Aplicacion/Validadores` y
  `Aplicacion/Mappers`. Errores en formato `application/problem+json` con un campo `codigo`
  estable y el mensaje en español.
- **Motivo**: evita dos dependencias (FluentValidation, AutoMapper) para una docena de DTOs.

## 14. Frontend

- **Decisión**: React 19 con TypeScript y Vite. React Router para las rutas. Sesión y tema en
  contextos de React; llamadas a la API con un cliente propio sobre `fetch`. CSS propio con
  variables. Pruebas con Vitest.
- **Estructura**: una sola aplicación en `frontend/`, con `src/plataforma/` (panel del
  DESARROLLADOR), `src/privado/` (aplicación del club), `src/compartido/` (sesión, tema, cliente
  de la API, componentes comunes) y, cuando llegue, `src/publico/`.
- **Nombres**: componentes, estados y funciones en español. Los hooks conservan el prefijo `use`
  que exige React (`useSesion`, `useTema`), igual que §2.1 permite la nomenclatura oficial de una
  tecnología.
- **Fuera de esta funcionalidad**: el empaquetado con Capacitor y la instalación como PWA. Nada
  de lo decidido aquí lo impide: no hay cookies y la dirección de la API es configurable.

## 15. Pruebas

- **Decisión**: xUnit. Las pruebas unitarias cubren las reglas de `Dominio/Reglas` y los
  servicios con repositorios simulados a mano. Las de integración levantan la API con
  `WebApplicationFactory` contra un PostgreSQL real en un contenedor (Testcontainers) y sustituyen
  el correo por una implementación en memoria que permite leer los enlaces enviados.
- **Motivo**: las reglas de esta funcionalidad son de autorización y aislamiento; solo son
  creíbles probadas contra la API y la base de datos reales (§20). El proveedor en memoria de EF
  no aplica índices únicos ni cascadas.

## 16. Foto de perfil

- **Decisión**: cada cuenta puede cargar, cambiar y quitar una foto de perfil desde la pantalla
  "Mi perfil". Mismas reglas que el escudo: PNG, JPEG o WebP, comprobados por su firma binaria,
  hasta 1 MB (pedido por el propietario el 2026-10-07). La validación es una sola clase,
  `ValidadorImagen`, compartida con el escudo. Se guarda en la tabla `FotoPerfil`, una fila por
  cuenta, separada de `Usuario`.
- **Quién la ve**: a diferencia del escudo, la foto es un dato personal y no se sirve sin sesión.
  En esta funcionalidad solo la ve su dueño, en la cabecera y en "Mi perfil" **(supuesto)**. El
  frontend la pide con la sesión y la muestra desde memoria.
- **Es opcional**: no se pide al registrarse. Sin foto se muestran las iniciales.
- **Pertenece a la cuenta**, no al club: es la misma en todos los clubes de la persona. Se borra
  con la cuenta. Si una cuenta llega a tener varios jugadores, la foto de cada jugador se decide
  en la funcionalidad de jugadores.
- **Alternativas**: servirla sin sesión como el escudo (expone fotos de personas a cualquiera que
  conozca un identificador); una foto por club (duplica el dato sin que nadie lo haya pedido).

## Cambios del contrato durante la implementación

`contracts/api.yaml` se ajustó en dos puntos al compararlo con el Swagger generado (T051). Ninguno
cambia una regla de negocio; el contrato describía de menos lo que la decisión 5 y la validación
de entrada (§23) ya pedían.

- **`429` en todos los endpoints anónimos de cuenta.** La decisión 5 limita las peticiones por IP
  en los endpoints anónimos de cuenta, pero el contrato solo lo decía en `POST /api/sesion` y
  `POST /api/cuenta/recuperacion`. Se añadió a `POST /api/cuenta/recuperacion/confirmacion`,
  `POST /api/invitaciones/consulta` y `POST /api/invitaciones/registro`, que también lo aplican:
  son los que reciben un token y los que más interesa proteger de intentos repetidos.
- **`400` en `PUT /api/plataforma/clubes/{clubId}/estado`.** Un cuerpo sin estado o con un estado
  que no existe responde `datos_invalidos`, como en el resto de la API.

## Supuestos a confirmar por el propietario

Ninguno bloquea el plan; si alguno cambia, el cambio es de configuración o de una validación.

| # | Supuesto | Dónde se usa |
| --- | --- | --- |
| 1 | La sesión dura 7 días y se renueva sola mientras se use la aplicación | Decisión 4 |
| 2 | Contraseña válida: entre 8 y 128 caracteres, sin más reglas | Decisión 5 |
| 3 | El enlace de recuperación de contraseña caduca a los 60 minutos | Decisión 5 |
| 4 | Los "datos de contacto" del club son un correo y un teléfono | Modelo de datos |
| 5 | El escudo de un club se sigue sirviendo aunque el club esté suspendido o dado de baja (lo necesitan el panel y el presidente). No decide nada sobre el sitio público, que sigue pendiente en §28 | Decisión 10 |
| 6 | La foto de perfil solo la ve su dueño; todavía no aparece en listados para otras personas | Decisión 16 |
