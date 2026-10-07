<!--
Informe de impacto de sincronización
- Cambio de versión: borrador previo sin aprobar → 1.0.0
- Origen: texto entregado por el propietario del proyecto el 2026-10-06. Sustituye por completo
  al borrador anterior de siete principios, que nunca fue aprobado.
- Principios modificados: todos (se reemplaza el documento completo)
- Secciones añadidas: 1 a 29 del texto del propietario
- Secciones eliminadas: los siete principios, "Restricciones del producto", "Flujo de trabajo" y
  "Governance" del borrador
- Añadido fuera del texto del propietario: únicamente la línea final de versión
- Pendientes: las decisiones abiertas están en la sección 28

Enmienda 1.0.0 → 1.1.0 (2026-10-06), decidida por el propietario:
- §4, §5, §6.2 y §20: carpetas, proyectos y capas pasan a nombres en español
- §8 y §12.2: toda cuenta entra como JUGADOR y el rol asignado lo reemplaza como único rol
- §8, §12.2 y §20: el DIRECTIVO aprueba cualquier ingreso y puede asignar el rol ENTRENADOR a la
  cuenta que acaba de aprobar
-->

# Constitución del Proyecto — Plataforma del Club Valfor F.C.

## 1. Propósito

Este proyecto es la plataforma web y móvil de Valfor F.C., escuela de fútbol infantil de Manizales
(Caldas, Colombia) con sede en la Cancha de Minitas. El club atiende jugadores desde los 6 años;
su categoría mayor es la 2012.

La plataforma tiene dos partes:

- Sitio público, sin inicio de sesión, para visitantes.
- Zona privada, con roles, para propietarios, entrenadores y jugadores. La cuenta del jugador es
  la que usa su familia.

La plataforma debe permitir:

- Mostrar al público la información del club: historia, sede, categorías, cuerpo técnico y
  contacto.
- Publicar resultados, torneos en los que participa el club, tablas de posiciones y goleadores.
- Gestionar jugadores, entrenadores y categorías.
- Gestionar mensualidades: cargos, pagos, abonos, saldos y estado de cuenta de cada jugador.
- Saber quién debe, cuánto debe y desde cuándo.
- Recibir pagos en línea mediante una pasarela de pago y registrar pagos en efectivo o por
  transferencia.
- Programar entrenamientos, partidos y torneos.
- Registrar convocatorias, asistencia y resultados.
- Gestionar el arbitraje de cada partido y su cobro entre los convocados.
- Mantener la ficha de cada jugador con acceso restringido.
- Enviar comunicados y mensajes de convocatoria.
- Mantener históricos deportivos y financieros sin alterar información histórica.

## 2. Principios obligatorios

### 2.1. Código en español

Todo el código nuevo debe utilizar nombres en español.

Esto incluye:

- Clases.
- Interfaces.
- Métodos.
- Propiedades.
- Variables.
- Parámetros.
- DTOs.
- Enumeraciones.
- Servicios.
- Repositorios.
- Controladores.
- Validadores.
- Mappers.
- Utilidades.
- Excepciones propias.
- Comentarios.
- Documentación XML.
- Componentes, hooks y estados del frontend.

Ejemplo:

```text
Jugador
JugadorDto
CrearJugadorDto
ActualizarJugadorDto
IJugadorServicio
JugadorServicio
IJugadorRepositorio
JugadorRepositorio
JugadorMapper
JugadorValidador
```

No utilizar nombres en inglés cuando exista una denominación clara en español.

Los nombres propios de tecnologías externas pueden mantenerse según su nomenclatura oficial, por
ejemplo:

- Entity Framework Core
- PostgreSQL
- ASP.NET Core
- JWT
- Swagger
- Docker
- xUnit
- React
- PSE

### 2.2. Vocabulario del dominio

Se utilizará siempre el mismo término para cada concepto:

| Término | Significado |
| --- | --- |
| Jugador | niño o niña inscrito en el club; su cuenta la usa su padre, madre o responsable |
| Entrenador | profesor a cargo de una o varias categorías |
| Propietario | presidente y dueño del club |
| Directivo | miembro de la directiva del club |
| Categoria | grupo de jugadores definido por año de nacimiento |
| Mensualidad | cobro mensual por la formación de un jugador |
| Cargo | valor que un jugador debe (mensualidad, arbitraje, otro) |
| Pago | dinero recibido y aplicado a uno o varios cargos |
| Convocatoria | lista de jugadores citados a un partido |
| Arbitraje | costo del árbitro de un partido |

No se usarán sinónimos en el código (por ejemplo Alumno, Profesor, Cuota) para estos conceptos.

No existe una entidad ni un rol Acudiente: el padre, la madre o el responsable usa la cuenta del
jugador. Sus datos de contacto son campos del Jugador.

### 2.3. Tamaño máximo de las clases

Ninguna clase puede superar las 250 líneas de código.

El límite aplica a todo archivo de código escrito a mano, tanto en el backend como en el frontend:
clases, servicios, controladores, repositorios y componentes.

Cuando una clase se acerca al límite, se divide por responsabilidades. No se permite eludir el
límite comprimiendo líneas, quitando documentación ni usando clases parciales.

Quedan exentos únicamente los archivos generados automáticamente, como las migraciones de Entity
Framework Core.

## 3. Documentación obligatoria de clases

Toda clase, interfaz, enum o componente de dominio relevante debe tener documentación XML en
español.

La documentación debe explicar:

- Qué representa.
- Cuál es su responsabilidad.
- Qué responsabilidades NO debe asumir.

Ejemplo:

```csharp
/// <summary>
/// Representa el servicio encargado de gestionar las mensualidades.
/// Su responsabilidad es generar los cargos mensuales, aplicar pagos
/// y calcular el saldo de cada jugador según las reglas de negocio.
/// No debe comunicarse directamente con la pasarela de pago
/// ni acceder al contexto de Entity Framework.
/// </summary>
public class MensualidadServicio
{
}
```

La documentación no debe ser decorativa. Debe ayudar a comprender la arquitectura.

## 4. Arquitectura

La aplicación utilizará una arquitectura por capas con separación clara de responsabilidades.

Estructura principal:

```text
src/
├── Valfor.Api/
│   └── Controladores/
│
├── Valfor.Aplicacion/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Servicios/
│   ├── Implementaciones/
│   ├── Mappers/
│   ├── Validadores/
│   └── Utilidades/
│
├── Valfor.Dominio/
│   ├── Entidades/
│   ├── Enumeraciones/
│   └── Reglas/
│
├── Valfor.Infraestructura/
│   ├── Datos/
│   ├── Repositorios/
│   └── Pagos/
│
└── Valfor.Web/
    ├── publico/
    └── privado/
pruebas/
├── Unitarias/
└── Integracion/
```

`Valfor.Infraestructura/Pagos` contiene la integración con la pasarela de pago, detrás de una
interfaz definida en Aplicacion.

`Valfor.Web` contiene el frontend: el sitio público y la zona privada.

El nombre definitivo de los proyectos podrá ajustarse durante el plan técnico, pero la separación
conceptual debe mantenerse.

## 5. Flujo de dependencias

El flujo normal de una operación será:

```text
Controlador
    ↓
IServicio
    ↓
Servicio
    ↓
IRepositorio
    ↓
Repositorio
    ↓
Entity Framework Core
    ↓
PostgreSQL
```

Para respuestas:

```text
Entidad
    ↓
Mapper
    ↓
DTO
    ↓
Controlador
    ↓
JSON
```

- Los controladores no deben contener reglas de negocio.
- Los repositorios no deben contener reglas de negocio propias de la aplicación.
- Los DTOs no deben utilizarse como entidades de persistencia.
- Las entidades de dominio no deben depender de ASP.NET Core.
- El frontend consume exclusivamente la API. No accede a la base de datos ni a la pasarela con
  credenciales privadas.

## 6. Tecnologías base

La primera implementación utilizará:

- C#.
- ASP.NET Core Web API.
- Entity Framework Core.
- PostgreSQL.
- JWT.
- Swagger/OpenAPI.
- Docker.
- xUnit.
- Git.
- GitHub.
- React para el frontend.
- Capacitor para empaquetar la aplicación Android.
- Wompi como pasarela de pago.

### 6.1. Una sola base de código para web, escritorio y móvil

El frontend es una única aplicación React adaptable.

Se distribuye de tres formas:

- Web: desde el navegador, en cualquier dispositivo.
- Escritorio: la misma web, instalable en el ordenador como aplicación (PWA).
- Android: la misma aplicación empaquetada con Capacitor y publicada en Google Play.

No se mantendrá una segunda base de código nativa para móvil.

iOS no forma parte de esta primera versión.

### 6.2. Pasarela de pago

Se utilizará Wompi, con sus medios de pago para Colombia (PSE, Nequi, tarjetas).

La integración queda detrás de una interfaz de Aplicacion (ver §4), de modo que cambiar de
pasarela no afecte las reglas de negocio.

El desarrollo y las pruebas se hacen contra el entorno de pruebas de la pasarela. Las llaves de
producción solamente se configuran en el despliegue.

No introducir nuevas tecnologías, frameworks o patrones arquitectónicos importantes sin justificar
su necesidad y documentar el cambio.

La simplicidad y mantenibilidad tienen prioridad sobre la cantidad de tecnologías utilizadas.

## 7. Un solo club

La plataforma atiende a un único club: Valfor F.C.

No se implementará multiempresa ni multiclub en esta versión.

Los datos propios del club (nombre, escudo, colores, sede, dirección, datos de contacto, valor de
la mensualidad) son configuración almacenada en base de datos o en archivos de configuración. No
deben quedar escritos de forma fija en el código.

El aislamiento de datos no es entre empresas, sino entre personas y categorías:

- Un entrenador solamente accede a las categorías que tiene asignadas.
- Una cuenta de jugador solamente accede a la información de su propio jugador. Cada familia ve
  únicamente lo de su hijo.

La aceptación del tratamiento de datos se da por hecha al inscribirse en el club. La plataforma no
gestiona autorizaciones ni consentimientos.

El backend debe aplicar este aislamiento.

No se puede confiar exclusivamente en restricciones del frontend.

El hecho de que un usuario conozca el identificador de otro jugador no le concede acceso.

## 8. Roles

Un visitante no es un rol: es cualquier persona sin sesión iniciada. Solamente accede al sitio
público (ver §9).

Existen cuatro roles con cuenta:

```text
PROPIETARIO
DIRECTIVO
ENTRENADOR
JUGADOR
```

JUGADOR es el rol por defecto de toda cuenta que se registra. Una cuenta nueva no tiene acceso a
nada hasta que su ingreso es aprobado (ver §12).

Una cuenta tiene un único rol. Toda cuenta entra como JUGADOR; cuando se le asigna el rol que le
corresponde (ENTRENADOR o DIRECTIVO), ese rol reemplaza al anterior y pasa a ser su único rol. Un
entrenador que además tiene un hijo en el club usa dos cuentas:
la suya, con su documento, como ENTRENADOR, y la del niño, con el documento del niño, como JUGADOR.

### Propietario

Es el presidente del club, que a su vez es su dueño. Administra el club completo.

Puede:

- Gestionar la configuración del club y el contenido del sitio público.
- Gestionar categorías, entrenadores y jugadores.
- Asignar entrenadores a categorías.
- Definir el valor de la mensualidad, descuentos y becas.
- Consultar las finanzas completas del club.
- Registrar, anular y corregir pagos.
- Gestionar torneos, partidos, arbitrajes y comunicados.
- Aprobar el ingreso de las cuentas que están en espera.
- Buscar usuarios registrados por su documento de identidad y asignarles o retirarles los roles
  ENTRENADOR o DIRECTIVO.

La asignación de roles es del PROPIETARIO, con una única excepción: un DIRECTIVO puede asignar el
rol ENTRENADOR a una cuenta cuyo ingreso acaba de aprobar (ver §12.2). Fuera de esa excepción,
ningún otro rol puede asignar, cambiar ni retirar roles, ni siquiera el suyo propio.

El rol PROPIETARIO no se asigna desde la aplicación.

El club debe conservar siempre al menos un PROPIETARIO activo; no se permite revocar el último.

### Directivo

Es un miembro de la directiva del club. El rol lo asigna el PROPIETARIO.

Puede:

- Consultar las finanzas del club: recaudo, quién debe y quién está al día.
- Consultar la documentación de cada jugador.
- Consultar la seguridad social de cada jugador: su entidad de salud y dónde lo atienden.
- Aprobar el ingreso de cualquier cuenta que está en espera.
- Asignar el rol ENTRENADOR a una cuenta cuyo ingreso acaba de aprobar.

Su acceso a finanzas y fichas es de consulta. No puede registrar ni anular pagos, modificar el
valor de la mensualidad, asignar el rol DIRECTIVO, retirar roles ni buscar usuarios por documento.

### Entrenador

El rol lo asigna el PROPIETARIO.

Está asignado a una o varias categorías mediante:

```text
AsignacionEntrenadorCategoria
```

Dentro de sus categorías puede:

- Consultar la ficha de sus jugadores, incluidos los datos médicos y de contacto de emergencia.
- Programar entrenamientos.
- Registrar asistencia.
- Crear convocatorias, seleccionando de la lista los jugadores a los que se envían.
- Registrar resultados, goles y demás eventos del partido.
- Registrar evaluaciones del jugador.
- Enviar comunicados a las familias de sus categorías.
- Consultar el estado de pago de sus jugadores.
- Registrar pagos en efectivo recibidos de sus jugadores.

Además dispone de un apartado de finanzas generales del club, de solo lectura: recaudo del mes,
cartera pendiente y totales por categoría.

No puede modificar el valor de la mensualidad, definir descuentos o becas, anular o corregir
pagos, ni gestionar jugadores de categorías que no tenga asignadas.

### Jugador

Es la cuenta de un jugador inscrito. El jugador es menor de edad, por lo que la cuenta la usa su
padre, madre o responsable. No existe un rol separado para la familia.

Cada jugador tiene su propia cuenta. Los hermanos no comparten cuenta: cada uno tiene la suya, con
su propio acceso.

Puede, únicamente sobre su propio jugador:

- Consultar la ficha, el calendario, las convocatorias, las estadísticas y las evaluaciones.
- Consultar el estado de cuenta y los recibos.
- Pagar mensualidades y arbitrajes.
- Confirmar asistencia a entrenamientos y partidos.
- Actualizar datos de contacto, médicos y documentos.

No accede a la ficha, los pagos ni los datos de contacto de otros jugadores.

Ve todos los entrenamientos de su categoría, pero solamente los partidos a los que fue convocado
(ver §17.2).

## 9. Sitio público

El sitio público no requiere inicio de sesión.

Solamente expone información que el club ha decidido publicar:

- Historia, sede, categorías, horarios y cuerpo técnico.
- Resultados de partidos ya jugados.
- Torneos en los que participa el club y tablas de posiciones.
- Goleadores del club.
- Formulario de contacto o de inscripción.

Los endpoints públicos son un conjunto separado y explícito de la API. Ningún endpoint privado
debe quedar accesible sin autenticación por omisión.

El sitio público no muestra los próximos partidos ni sus convocatorias (ver §17.2).

De un jugador, el sitio público solamente muestra su nombre, su categoría y sus estadísticas
deportivas.

Los endpoints públicos nunca devuelven documento, datos de contacto, datos médicos ni información
de pagos.

## 10. Identidad mediante documento

El número de documento de identidad identifica de forma única a una persona dentro de la
plataforma.

Se almacena junto con su tipo:

```text
REGISTRO_CIVIL
TARJETA_IDENTIDAD
CEDULA_CIUDADANIA
CEDULA_EXTRANJERIA
```

El documento:

- Es único globalmente.
- No puede duplicarse.
- No debe utilizarse como sustituto de las claves primarias internas.

Un jugador puede pasar de registro civil a tarjeta de identidad. Ese cambio actualiza el tipo y el
número sobre el mismo registro; no crea otra persona.

Las relaciones entre entidades utilizarán IDs internos.

## 11. Categorías y cambio de categoría

Las categorías se definen por año de nacimiento: cada año es una categoría (2012, 2013, 2014,
2015, etc.). Actualmente el club cubre de la 2012 a la 2020.

### 11.1. Actividades conjuntas

En ocasiones varias categorías se juntan para un entrenamiento, un partido o una convocatoria.

Por eso un entrenamiento, un partido, una convocatoria o un mensaje de convocatoria puede
dirigirse a una o a varias categorías.

Juntar categorías para una actividad no cambia la categoría de ningún jugador ni crea una
categoría nueva.

### 11.2. Cambio de categoría

Debe existir un único registro de Jugador por persona.

Un jugador pertenece a una única categoría actual:

```text
Jugador.CategoriaId
```

Cambiar la categoría de un jugador es una operación del PROPIETARIO.

Al cambiar de categoría:

- No se crea otro jugador.
- No se modifica información histórica.
- Los partidos, convocatorias, asistencias, goles y cargos anteriores conservan la categoría en la
  que ocurrieron.

No se implementará inicialmente una entidad independiente de historial de categorías del jugador.

## 12. Usuario, registro y roles

La entidad Usuario representa la cuenta de acceso.

### 12.1. Registro

Cualquier persona puede registrarse en la aplicación.

- La cuenta de un jugador se registra con el documento del jugador (el niño), no con el de su
  familia.
- Un entrenador o un directivo se registra con su propio documento.

Toda cuenta nueva recibe el rol JUGADOR y queda en espera.

#### 12.1.1. Sala de espera

Una cuenta en espera solamente ve una pantalla que le indica que su ingreso está pendiente de
aprobación.

Mientras está en espera:

- No accede a ninguna información del club: ni entrenamientos, ni horarios, ni jugadores, ni
  pagos.
- No se le asigna categoría.
- No se le genera mensualidad.

El ingreso lo aprueba el PROPIETARIO o un DIRECTIVO.

Al aprobarse el ingreso de un jugador, se le asigna la categoría que corresponde a su año de
nacimiento y empieza a generarse su mensualidad.

El estado de ingreso de una cuenta es:

```text
EN_ESPERA
APROBADO
```

### 12.2. Asignación de roles

El PROPIETARIO busca a un usuario ya registrado por su documento de identidad y decide:

- Asignarle el rol ENTRENADOR.
- Asignarle el rol DIRECTIVO.
- Dejarlo como está, es decir, como JUGADOR.

El rol asignado reemplaza al rol JUGADOR con el que entró la cuenta y pasa a ser su único rol.

Un DIRECTIVO, después de aprobar un ingreso, puede asignar a esa cuenta el rol ENTRENADOR. No
puede asignar el rol DIRECTIVO ni retirar roles.

Un rol solamente puede asignarse a un usuario que ya se registró. No se asignan roles a documentos
que aún no tienen cuenta.

La búsqueda de usuarios por documento es exclusiva del PROPIETARIO.

### 12.3. Alcance de cada rol

Los roles se resuelven mediante:

```text
Usuario
   ↓
UsuarioRol
```

Usuario no contiene CategoriaId.

El alcance de cada rol se resuelve exclusivamente así:

- PROPIETARIO: alcance global.
- DIRECTIVO: consulta global, con los límites definidos en §8.
- ENTRENADOR: mediante AsignacionEntrenadorCategoria. No se crea un UsuarioRol ENTRENADOR por cada
  categoría.
- JUGADOR: mediante Jugador.UsuarioId. La relación es uno a uno: una cuenta corresponde a un único
  jugador. Los hermanos tienen cuentas separadas.

La ficha de Jugador se crea a partir del registro de la cuenta.

## 13. Histórico operativo y financiero

La información histórica no debe depender exclusivamente del estado actual de una entidad.

Los datos que puedan cambiar con el tiempo y sean relevantes para un registro histórico deben
quedar almacenados en ese registro.

Por ejemplo:

```text
ConfiguracionClub.ValorMensualidad
```

representa el valor vigente.

Mientras que:

```text
Cargo.Valor
```

representa el valor cobrado específicamente en ese mes a ese jugador.

Modificar el valor de la mensualidad no debe modificar cargos ya generados.

Del mismo modo:

```text
PartidoJugador.CategoriaId
```

conserva la categoría con la que el jugador disputó ese partido, aunque después cambie de
categoría.

## 14. Entidades desactivables

Las entidades con información histórica no deben eliminarse físicamente cuando dejan de estar
disponibles.

Se utilizará el concepto de:

```text
Activo
Activa
```

según corresponda.

Esto aplica, entre otras, a:

- Jugadores.
- Entrenadores.
- Categorías.
- Torneos.
- Sedes o canchas.
- Asignaciones entre entrenador y categoría.

Un jugador retirado se desactiva; su historial deportivo y financiero se conserva.

La eliminación física deberá justificarse explícitamente.

## 15. Reglas de negocio en backend

Las reglas de negocio deben estar protegidas en el backend.

El frontend puede ayudar con validaciones de experiencia de usuario, pero nunca será la única
barrera de seguridad o integridad.

Ejemplo:

```text
React → oculta botón
```

no es suficiente.

Debe existir:

```text
API → valida autorización y regla de negocio
```

## 16. Mensualidades, cargos y pagos

### 16.1. Cargos y saldo

Todo lo que un jugador debe se representa como un Cargo:

```text
MENSUALIDAD
ARBITRAJE
OTRO
```

Todo dinero recibido se representa como un Pago, aplicado a uno o varios cargos.

El saldo de un jugador se calcula a partir de sus cargos y pagos. No existe un campo de saldo
editable a mano.

El estado de cuenta ("al día", "pago parcial", "debe") se deriva de ese cálculo.

Se permiten abonos parciales.

Los descuentos y becas se aplican al generar el cargo y quedan registrados en él con su motivo.

### 16.2. Dinero

Los valores se manejan en pesos colombianos (COP) con tipo decimal. Nunca con float ni double.

### 16.3. Inmutabilidad

Un pago confirmado no se edita ni se elimina.

Un error se corrige mediante una anulación, que queda registrada con autor, fecha y motivo.

Todo pago registra quién lo registró, cuándo y por qué medio:

```text
PASARELA
TRANSFERENCIA
EFECTIVO
```

### 16.4. Pasarela de pago

La plataforma nunca almacena ni procesa números de tarjeta, códigos de seguridad ni credenciales
bancarias. El pago ocurre en la pasarela.

Un pago en línea solamente se considera confirmado cuando el backend recibe y verifica la
notificación de la pasarela. El retorno del navegador del usuario no es prueba de pago.

El procesamiento de notificaciones debe ser idempotente: recibir dos veces la misma notificación
no puede registrar dos pagos.

Las llaves privadas de la pasarela son secretos de configuración. No se incluyen en el repositorio
ni en el frontend.

### 16.5. Transferencia y efectivo

Un pago por transferencia queda en revisión hasta que un PROPIETARIO lo confirma.

Un pago en efectivo lo registra un PROPIETARIO o el ENTRENADOR de la categoría del jugador.

Todo pago confirmado genera un recibo consultable desde la cuenta del jugador.

### 16.6. Arbitraje

El costo del arbitraje de un partido se reparte entre los jugadores convocados y genera un Cargo
de tipo ARBITRAJE para cada uno.

### 16.7. Mensualidad

La mensualidad se paga durante los primeros cinco días de cada mes.

El valor vigente es de $65.000 COP por jugador. Es un dato de configuración que el PROPIETARIO
puede actualizar; no debe quedar escrito de forma fija en el código.

Un cambio de valor aplica a los cargos que se generen después del cambio. Los cargos ya generados
conservan su valor (ver §13).

Cada mes se genera un cargo de tipo MENSUALIDAD por cada jugador activo.

Una mensualidad sin pagar después del día 5 simplemente aparece como pendiente en el estado de
cuenta. Es un dato informativo y no tiene consecuencias.

No existe recargo por mora. Deber no genera intereses, cargos adicionales ni restricciones de
ningún tipo para el jugador: no afecta su acceso, sus entrenamientos ni sus convocatorias.

### 16.8. Recordatorios

En esta primera versión no se envían recordatorios de pago, ni automáticos ni manuales. El estado
de cuenta se consulta dentro de la aplicación.

## 17. Datos deportivos

Las estadísticas se calculan a partir de lo registrado en cada partido. No se digitan como
totales.

Esto aplica a:

- Goles y goleadores.
- Partidos jugados.
- Asistencia a entrenamientos.

Corregir un partido corrige automáticamente las estadísticas derivadas.

El sistema no debe modificar automáticamente una convocatoria, un resultado o una programación
registrados manualmente por un entrenador o propietario sin una regla explícita que lo autorice.

### 17.1. Tablas de posiciones

Las tablas de posiciones de los torneos son la excepción: en esta primera versión se digitan
manualmente, por torneo y categoría, por un PROPIETARIO o por el ENTRENADOR de la categoría.

Cada tabla registra quién la actualizó por última vez y cuándo.

La actualización automática desde una fuente de la liga no forma parte de esta versión. Si más
adelante se incorpora, debe convivir con la edición manual y no sobrescribir una corrección manual
sin una regla explícita.

### 17.2. Convocatorias y visibilidad de los partidos

Ningún niño debe enterarse por la plataforma de que no fue convocado. Esta regla prevalece sobre
la comodidad de mostrar el calendario completo.

#### Convocatoria a partidos

El entrenador crea la convocatoria seleccionando, de la lista de jugadores, a quiénes se les
envía.

Un partido próximo y su convocatoria solamente son visibles para:

- Los jugadores convocados a ese partido.
- Los entrenadores de las categorías involucradas.
- Los propietarios.

Para una cuenta no convocada, ese partido no existe: no aparece en su calendario, ni en listados,
ni en contadores, ni en mensajes, ni puede consultarse por su identificador.

Nadie puede ver desde una cuenta de jugador la lista de los demás convocados.

El cargo de arbitraje solamente se genera para los convocados.

#### Convocatoria a entrenamientos

Los entrenamientos son visibles para todos los jugadores de las categorías a las que van
dirigidos, sin excepción.

#### Partidos ya jugados

Una vez jugado, el resultado del partido es público y alimenta las estadísticas (goles,
goleadores, posiciones).

El backend aplica esta visibilidad. No basta con ocultar los partidos en el frontend.

## 18. Estados

Los estados representan el estado actual de una entidad.

No se debe crear automáticamente un historial de estados para cada entidad.

Para esta primera versión se utilizará únicamente el estado actual, con una excepción expresa: los
pagos y sus anulaciones conservan siempre su trazabilidad completa (ver §16.3).

## 19. Simplicidad

No sobreingenierizar.

Antes de crear:

- Una nueva entidad.
- Un nuevo servicio.
- Un nuevo patrón.
- Una nueva abstracción.
- Una nueva tecnología.
- Una nueva tabla.

debe existir una necesidad funcional o técnica clara.

El sistema es para un club con decenas de jugadores, no para miles. La arquitectura debe ser
suficientemente sólida para crecer, pero no innecesariamente compleja.

## 20. Pruebas

Las reglas de negocio importantes deben tener pruebas automatizadas.

Se utilizarán:

```text
pruebas/Unitarias
pruebas/Integracion
```

Las pruebas deben validar principalmente:

- Generación de cargos y cálculo de saldos.
- Aplicación de pagos, abonos parciales y anulaciones.
- Idempotencia de las notificaciones de la pasarela.
- Autorización por rol.
- Que solamente el PROPIETARIO puede buscar usuarios por documento, asignar el rol DIRECTIVO y
  retirar roles.
- Que un DIRECTIVO solamente puede asignar el rol ENTRENADOR, y solo a una cuenta cuyo ingreso
  aprobó.
- Que una cuenta nunca tiene más de un rol.
- Que una cuenta en espera no accede a ninguna información del club ni genera mensualidad.
- Que solamente el PROPIETARIO o un DIRECTIVO pueden aprobar un ingreso.
- Aislamiento entre categorías y entre cuentas de jugador.
- Que una cuenta no convocada no puede ver ni consultar un partido próximo, y que sí ve todos los
  entrenamientos de su categoría.
- Que los endpoints públicos no exponen documento, contacto, datos médicos ni pagos.
- Cálculo de estadísticas deportivas.
- Transiciones de estados.
- Persistencia e integración con PostgreSQL cuando corresponda.

## 21. Docker

La aplicación debe poder ejecutarse mediante Docker.

La configuración de Docker debe permitir reproducir el entorno de desarrollo y facilitar
posteriormente el despliegue.

No se deben introducir dependencias externas innecesarias únicamente para dockerizar el proyecto.

## 22. Migraciones de base de datos

Entity Framework Core será responsable de gestionar el modelo de persistencia y las migraciones.

Las modificaciones estructurales de la base de datos deben realizarse mediante migraciones
controladas.

No se deben realizar cambios manuales arbitrarios en producción como mecanismo habitual de
evolución del esquema.

## 23. API

La API debe utilizar:

- HTTP semántico.
- DTOs.
- Validación de entrada.
- Respuestas coherentes.
- Autorización.
- Manejo controlado de errores.
- Swagger/OpenAPI.

No exponer directamente entidades de persistencia como contrato público de la API cuando un DTO
sea apropiado.

Los DTOs públicos y los privados de una misma entidad son tipos distintos. Un DTO público de
jugador no contiene campos privados, ni siquiera vacíos.

## 24. Diseño visual

El diseño aprobado en el lienzo del proyecto es la referencia visual:

<https://claude.ai/artifact/PiDB5BQXRYCeN6nnB4TikR>

La interfaz usa la identidad del club: escudo de Valfor F.C. y colores naranja, vinotinto, dorado
y negro.

Toda pantalla debe funcionar en teléfono y en escritorio.

En el diseño, las pantallas móviles rotuladas para "acudientes" corresponden a la cuenta del
jugador.

Los estados de pago deben distinguirse por texto además de por color.

## 25. Regla para Claude Code

Claude Code actúa como implementador de las decisiones definidas en la especificación.

No debe cambiar unilateralmente decisiones de negocio ya aprobadas.

Si detecta una contradicción, ambigüedad o requisito faltante que afecte significativamente la
arquitectura o el comportamiento:

- Debe identificar el problema.
- Debe documentarlo.
- Debe detener la implementación de esa parte.
- Debe solicitar una decisión antes de inventar una regla de negocio.

No debe introducir funcionalidades no solicitadas bajo la premisa de que "podrían ser útiles".

Los valores de ejemplo del diseño (nombres de jugadores, cifras, torneos, valor de la mensualidad)
no son reglas de negocio ni datos reales.

## 26. Trazabilidad

Cada funcionalidad implementada debe poder relacionarse con:

```text
Requisito
   ↓
Especificación
   ↓
Plan
   ↓
Tarea
   ↓
Código
   ↓
Prueba
```

Las tareas deben ser suficientemente concretas para comprobar si fueron completadas.

## 27. Definición de terminado y no regresión

### 27.1. Backend y frontend se construyen juntos

Cada funcionalidad se construye completa, de extremo a extremo: su parte de backend y su pantalla
de frontend se hacen en la misma tarea.

No se avanza a la siguiente funcionalidad dejando un endpoint sin pantalla que lo use, ni una
pantalla sin su endpoint real.

No se construye primero todo el backend y después todo el frontend.

### 27.2. Criterios

Una tarea no se considera terminada únicamente porque el código compile.

Cuando corresponda, debe incluir:

- Implementación del backend.
- Pantalla de frontend que usa esa funcionalidad contra la API real.
- Documentación en español de cada clase (ver §3).
- Ninguna clase por encima de 250 líneas (ver §2.3).
- Validaciones.
- Pruebas.
- Integración.
- Documentación.
- Migraciones.
- Actualización de contratos.
- Verificación de compilación.
- Verificación de pruebas.

Una tarea debe marcarse como completada solamente después de verificar sus criterios.

Una modificación no debe romper funcionalidades previamente implementadas.

Antes de considerar una funcionalidad terminada se debe ejecutar la batería de pruebas relevante.

Cuando sea necesario, se deben agregar pruebas para evitar que el defecto reaparezca.

## 28. Decisiones pendientes

Estas decisiones no están tomadas. Según §25, no deben resolverse por cuenta propia durante la
implementación.

- **Registro en la pasarela.** A nombre de quién se abre la cuenta de Wompi y en qué cuenta
  bancaria se recibe el dinero.
- **Matrícula, descuentos y becas.** Si existe un cobro de inscripción y si hay descuentos (por
  ejemplo, por hermanos) o becas.
- **Fuente automática de posiciones.** Todavía no se conoce ningún enlace o servicio de las ligas
  del que se puedan leer las tablas. Mientras no exista, se mantienen manuales (ver §17.1).
- **Recordatorios de pago.** Quedan fuera de esta versión (ver §16.8). Si más adelante se quieren,
  habrá que decidir el canal.
- **Registros que no se aprueban.** Qué pasa con una cuenta en espera que el club no quiere
  aceptar: si se puede rechazar, si se borra y si esa persona puede volver a registrarse.

## 29. Principio final

El sistema debe priorizar:

```text
Claridad
↓
Correctitud
↓
Seguridad
↓
Mantenibilidad
↓
Escalabilidad
```

La solución más sencilla que cumpla correctamente las reglas de negocio será preferible a una
solución más compleja.

**Versión**: 1.1.0 | **Ratificada**: 2026-10-06 | **Última enmienda**: 2026-10-06
