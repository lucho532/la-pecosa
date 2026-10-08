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

Enmienda 1.1.0 → 1.2.0 (2026-10-06), decidida por el propietario del proyecto:
- Todo el documento: el rol PROPIETARIO pasa a llamarse PRESIDENTE
- §4 y §20: el repositorio se divide en backend/ y frontend/; desaparece Valfor.Web
- §2.2, §8, §12.3 y §12.5: nuevo rol único DESARROLLADOR, que crea usuarios con rol directo y
  contraseña por defecto de un solo uso
- §12.2: la ficha de Jugador se elimina al pasar la cuenta a ENTRENADOR o DIRECTIVO
- §6, §6.3 y §12.4: inicio de sesión completo para todos y Brevo para los correos de la cuenta
- §12.1: el registro pide correo electrónico
- §28: dos decisiones pendientes nuevas

Enmienda 1.2.0 → 2.0.0 (2026-10-07), decidida por el propietario del proyecto. Cambio mayor: se
redefine el principio de un solo club.
- Título y §1: la plataforma pasa a ser multiclub; Valfor F.C. es el primer club
- §7: "Un solo club" se reemplaza por "Varios clubes", con aislamiento entre clubes e identidad
  (escudo y colores) por club
- §8, §12.3 y §12.5: el DESARROLLADOR tiene un panel de administración de la plataforma; crea
  clubes, cambia su escudo y colores y crea sus usuarios; no accede a nada más
- §2.2: nuevo término Club
- §9, §16.7, §19 y §24: ajustes para varios clubes
- §20: pruebas de aislamiento entre clubes y del alcance del DESARROLLADOR
- §28: se cierra "Alcance del DESARROLLADOR" y se abren seis decisiones nuevas

Enmienda 2.0.0 → 2.1.0 (2026-10-07), decidida por el propietario del proyecto:
- §12.1 y §6.3: el registro pasa a ser por invitación enviada por correo
- §7.2 y §7.3: cada usuario guarda el club al que pertenece y ve su nombre; el resto de la
  configuración la editan el DESARROLLADOR y el PRESIDENTE
- §10: el documento es único por pareja "documento + club"
- §6.2: cada club recibe su dinero en su propia cuenta de la pasarela
- §7.4: suspensión y baja de clubes; congelación automática por impago de la plataforma
- §24: tema claro y oscuro con botón
- §20: pruebas nuevas
- §28: se cierran cinco decisiones y se abren seis de detalle

Enmienda 2.1.0 → 3.0.0 (2026-10-07), decidida por el propietario del proyecto. Cambio mayor: se
elimina la creación de usuarios con contraseña por defecto y el rol pasa a ser por club.
- §12.5: el DESARROLLADOR ya no crea usuarios ni asigna contraseñas; al crear un club invita
  obligatoriamente a su PRESIDENTE por correo
- §12.1: datos de registro (nombre, apellidos, correo del acudiente, documento, fecha de
  nacimiento)
- §12.4: inicio de sesión con correo o documento
- §7.3, §8 y §10: una persona puede pertenecer a varios clubes, con un rol en cada uno, un solo
  inicio de sesión y un desplegable para elegir club
- §7.4: estados suspendido, dado de baja y eliminado; el DESARROLLADOR levanta y aplica la
  suspensión manualmente; eliminar borra toda la información del club
- §20: pruebas ajustadas
- §28: se cierran dos decisiones y se abren cinco

Enmienda 3.0.0 → 3.1.0 (2026-10-07), decidida por el propietario del proyecto:
- §12.1: el registro vuelve a pedir celular y nombre del responsable; se explicitan tipo de
  documento y contraseña
- §8 y §12.4: con correo compartido se elige el integrante; con documento se ve solo ese
  integrante
- §12.4: una persona puede solicitar la eliminación de su cuenta
- §8: un PRESIDENTE no puede quitarse el rol ni eliminarse sin otro PRESIDENTE
- §6.1: una sola aplicación Android para todos los clubes
- §7.4: se confirma que dar de baja y eliminar son dos pasos
- §28: se cierran cinco decisiones y se abren tres

Enmienda 3.1.0 → 3.2.0 (2026-10-07), decidida por el propietario del proyecto:
- §12.1: el correo es único; no se registra una cuenta nueva con un correo ya registrado
- §12.1.2 (nueva), §8 y §12.4: los hermanos se agregan desde la ficha del jugador ya registrado y
  comparten los datos de contacto; una cuenta tiene un correo, una contraseña y varios jugadores
- §12.4: la eliminación de la cuenta se cumple sola y conserva los pagos
- §8 y §12.5: un club puede tener varios presidentes; al adicional lo elige el DESARROLLADOR u
  otro PRESIDENTE del club
- §28: se cierran tres decisiones y se abren tres de detalle

Enmienda 3.2.0 → 3.3.0 (2026-10-07), decidida por el propietario del proyecto:
- §12.1.2: el hermano agregado desde la ficha pasa por la sala de espera
- §12.4: de una cuenta eliminada solo se conservan los pagos con nombre completo y documento; se
  eliminan sus estadísticas deportivas
- §12.5: un PRESIDENTE elige a otro dándole el rol a alguien ya registrado en el club
- §28: se cierran las tres decisiones de detalle

Enmienda 3.3.0 → 3.4.0 (2026-10-07), decidida por el propietario del proyecto en las aclaraciones
de la spec 001:
- §8: el DESARROLLADOR invita presidentes adicionales, quita el rol a un presidente (eligiendo
  entre darle otro rol o eliminarlo del club) y revierte la baja de un club; solo se elimina un
  club dado de baja
- §12.4: bloqueo de la cuenta tras 5 intentos fallidos, hasta recuperar la contraseña por correo

Enmienda 3.4.0 → 3.5.0 (2026-10-07), decidida por el propietario del proyecto:
- Título y §1: el proyecto pasa a llamarse La Pecosa; Valfor F.C. es solo su primer cliente
- §1: formas del nombre sin espacios (LaPecosa, la-pecosa, lapecosa)
- §4: los proyectos del backend pasan de Valfor.* a LaPecosa.*

Enmienda 3.5.0 → 3.6.0 (2026-10-07), decidida por el propietario del proyecto tras el análisis de
la spec 001:
- §7.1: además del panel del DESARROLLADOR, se admiten como excepciones acotadas las consultas de
  la propia cuenta (iniciar sesión y listar sus clubes) y abrir una invitación por su enlace

Enmienda 3.6.0 → 3.7.0 (2026-10-07), decidida por el propietario del proyecto en las aclaraciones
de la spec 002:
- §8 y §12.1: dentro del club invitan el PRESIDENTE y los DIRECTIVOS; toda invitación sirve una
  sola vez, caduca y queda ligada a su correo
- §12.1 y §12.1.1: quien se registra con una invitación del club pasa siempre por la sala de
  espera
- §8 y §12.1.1: el PRESIDENTE o un DIRECTIVO pueden rechazar un ingreso en espera; el rechazo
  borra a la persona del club y solo puede volver con una invitación nueva
- §14: el rechazo de un ingreso en espera es una eliminación física justificada
- §20: pruebas nuevas
- §28: se cierran tres decisiones
- Principios modificados: ninguno renombrado. Secciones añadidas o eliminadas: ninguna
- Pendientes: ninguno nuevo; siguen abiertas las siete decisiones restantes de la §28
-->

# Constitución del Proyecto — La Pecosa, plataforma multiclub de escuelas de fútbol

## 1. Propósito

Este proyecto se llama **La Pecosa**. Es una plataforma web y móvil para escuelas y clubes de
fútbol. Valfor F.C. no es el nombre del proyecto: es uno de sus clientes, el primero.

El nombre visible para las personas es "La Pecosa". En todo lugar donde un espacio o una
mayúscula pueda dar problemas se usa una de estas dos formas, y ninguna otra:

- `LaPecosa`: proyectos, espacios de nombres y clases del backend (por ejemplo `LaPecosa.Api`).
- `la-pecosa`: carpeta del proyecto, repositorio, paquetes del frontend, imágenes de Docker y
  direcciones web. Donde no se admita el guion (identificador de la aplicación Android, nombre de
  la base de datos) se usa `lapecosa`.

Es multiclub: una
misma instalación atiende a varios clubes, cada uno con sus propios datos, su escudo y sus colores
(ver §7).

El primer club es Valfor F.C., escuela de fútbol infantil de Manizales (Caldas, Colombia) con sede
en la Cancha de Minitas. Atiende jugadores desde los 6 años; su categoría mayor es la 2012.

La plataforma tiene tres partes:

- Panel de administración de la plataforma, exclusivo del DESARROLLADOR, desde donde se crean los
  clubes, se configura su escudo y sus colores y se crean sus usuarios.
- Sitio público de cada club, sin inicio de sesión, para visitantes.
- Zona privada, con roles, para el presidente, directivos, entrenadores y jugadores. La cuenta del jugador es
  la que usa su familia.

La plataforma debe permitir, para cada club:

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
| Presidente | presidente y dueño del club |
| Club | escuela o equipo que usa la plataforma; todos sus datos le pertenecen solo a él |
| Desarrollador | dueño de la plataforma; crea los clubes y sus usuarios desde su panel |
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

El repositorio tiene dos carpetas principales y separadas: `backend/` para la API y `frontend/`
para la aplicación React. Ninguna contiene código de la otra.

Estructura principal:

```text
backend/
├── src/
│   ├── LaPecosa.Api/
│   │   └── Controladores/
│   │
│   ├── LaPecosa.Aplicacion/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Servicios/
│   │   ├── Implementaciones/
│   │   ├── Mappers/
│   │   ├── Validadores/
│   │   └── Utilidades/
│   │
│   ├── LaPecosa.Dominio/
│   │   ├── Entidades/
│   │   ├── Enumeraciones/
│   │   └── Reglas/
│   │
│   └── LaPecosa.Infraestructura/
│       ├── Datos/
│       ├── Repositorios/
│       ├── Pagos/
│       └── Correo/
│
└── pruebas/
    ├── Unitarias/
    └── Integracion/

frontend/
├── publico/
└── privado/
```

`LaPecosa.Infraestructura/Pagos` contiene la integración con la pasarela de pago y
`LaPecosa.Infraestructura/Correo` la integración con el servicio de correo. Ambas quedan detrás de
interfaces definidas en Aplicacion.

`frontend/` contiene la aplicación React: el sitio público y la zona privada.

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
- Brevo para el envío de correos.

### 6.1. Una sola base de código para web, escritorio y móvil

El frontend es una única aplicación React adaptable.

Se distribuye de tres formas:

- Web: desde el navegador, en cualquier dispositivo.
- Escritorio: la misma web, instalable en el ordenador como aplicación (PWA).
- Android: la misma aplicación empaquetada con Capacitor y publicada en Google Play. Es una sola
  aplicación para todos los clubes; el escudo y los colores de cada club aparecen al iniciar
  sesión.

No se mantendrá una segunda base de código nativa para móvil.

iOS no forma parte de esta primera versión.

### 6.2. Pasarela de pago

Se utilizará Wompi, con sus medios de pago para Colombia (PSE, Nequi, tarjetas).

La integración queda detrás de una interfaz de Aplicacion (ver §4), de modo que cambiar de
pasarela no afecte las reglas de negocio.

Cada club recibe su dinero en su propia cuenta de la pasarela. Las llaves de la pasarela de cada
club son secretos de ese club: se guardan protegidas, nunca se devuelven por la API y nunca llegan
al frontend.

El pago de cada club por el uso de la plataforma se cobra con la misma pasarela (ver §7.4).

El desarrollo y las pruebas se hacen contra el entorno de pruebas de la pasarela. Las llaves de
producción solamente se configuran en el despliegue.

### 6.3. Correo

Se utilizará Brevo para enviar los correos de la plataforma: las invitaciones para registrarse en
un club (ver §12.1) y los de la cuenta, como el de recuperación de contraseña (ver §12.4).

La integración queda detrás de una interfaz de Aplicacion (ver §4). La llave de Brevo es un
secreto de configuración: no se incluye en el repositorio ni en el frontend.

No introducir nuevas tecnologías, frameworks o patrones arquitectónicos importantes sin justificar
su necesidad y documentar el cambio.

La simplicidad y mantenibilidad tienen prioridad sobre la cantidad de tecnologías utilizadas.

## 7. Varios clubes

La plataforma es multiclub: atiende a varios clubes desde una misma instalación. Valfor F.C. es el
primero.

### 7.1. Aislamiento entre clubes

Todo dato de un club (usuarios, jugadores, categorías, cargos, pagos, partidos, torneos,
comunicados, configuración) pertenece a exactamente un club.

Un usuario de un club nunca accede a datos de otro club: ni los ve, ni los cuenta, ni puede
consultarlos por su identificador.

Ninguna consulta ni operación puede ejecutarse sin estar limitada a un club. Las únicas
excepciones son:

- El panel de administración del DESARROLLADOR (ver §8).
- Las consultas de la propia cuenta, necesarias para iniciar sesión y elegir club (ver §7.3 y
  §12.4): encontrar la cuenta por su correo o su documento y listar los clubes a los que
  pertenece. Siempre quedan limitadas a esa cuenta.
- Abrir una invitación por su enlace (ver §12.1): devuelve solo esa invitación y su club.

Ninguna excepción entrega datos de un club a quien no pertenece a él.

### 7.2. Identidad y configuración de cada club

Los datos propios de cada club (nombre, escudo, colores, sede, dirección, datos de contacto, valor
de la mensualidad) son configuración de ese club almacenada en base de datos. No deben quedar
escritos de forma fija en el código, y el código no debe asumir que el club es Valfor F.C.

El escudo y los colores de un club los configura el DESARROLLADOR desde su panel de
administración. La interfaz de cada club se muestra con su propia identidad.

El resto de la configuración del club (nombre, sede, dirección, datos de contacto) la pueden
editar tanto el DESARROLLADOR como el PRESIDENTE de ese club.

Todo usuario ve en pantalla el nombre de su club.

### 7.3. Pertenencia a un club

Cada integrante guarda el identificador del club al que pertenece. Ese identificador se fija al
registrarse mediante la invitación (ver §12.1) y determina qué club ve.

Una misma persona puede ser integrante de varios clubes. Inicia sesión una sola vez y, dentro de
su panel, un desplegable le permite elegir de cuál club quiere ver los datos. En cada momento ve
únicamente los datos del club elegido, y solo puede elegir entre los clubes a los que pertenece.

### 7.4. Suspensión, baja, eliminación y pago por el uso de la plataforma

Cada club paga por el uso de la plataforma, mediante la misma pasarela de pago (ver §6.2).

**Suspendido (congelado).** Si un club no ha hecho ese pago, queda suspendido automáticamente
hasta ponerse al día. En cuanto el pago se confirma, el club vuelve a la normalidad de inmediato.
El DESARROLLADOR también puede, desde su panel, suspender un club manualmente, levantar una
suspensión y volver a aplicarla.

Mientras un club está suspendido:

- Solamente entra su PRESIDENTE.
- Cualquier otro integrante que intente entrar a ese club ve un aviso de incidencia temporal que
  le pide comunicarse con el presidente.
- No pasa nada más: todos los datos del club permanecen intactos.

**Dado de baja.** Lo aplica el DESARROLLADOR desde su panel. Nadie del club puede entrar, ni
siquiera su PRESIDENTE.

**Eliminado.** Eliminar un club borra todo rastro e información de ese club, sin posibilidad de
recuperarla. Es la única excepción a la conservación de históricos y a la inmutabilidad de los
pagos (ver §13, §14 y §16.3), solo puede hacerla el DESARROLLADOR y exige una confirmación
expresa.

### 7.5. Aislamiento dentro de un club

Dentro de un club, el aislamiento es entre personas y categorías:

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

Existen cinco roles con cuenta:

```text
DESARROLLADOR
PRESIDENTE
DIRECTIVO
ENTRENADOR
JUGADOR
```

JUGADOR es el rol por defecto de toda cuenta que se registra por sí misma. Esa cuenta no tiene
acceso a nada hasta que su ingreso es aprobado (ver §12). Quien se registra con una invitación del
DESARROLLADOR entra directamente con el rol de esa invitación (ver §12.5).

Dentro de un club, un integrante tiene un único rol. Una persona que pertenece a varios clubes
tiene un rol en cada uno, y pueden ser distintos. Toda cuenta entra como JUGADOR; cuando se le asigna el rol que le
corresponde (ENTRENADOR o DIRECTIVO), ese rol reemplaza al anterior y pasa a ser su único rol. Un
entrenador que además tiene un hijo en el club usa dos cuentas:
la suya, con su documento, como ENTRENADOR, y la del niño, con el documento del niño, como JUGADOR.

### Desarrollador

Es el dueño de la plataforma, que la ofrece a varios clubes. Es un rol único: existe una sola
cuenta DESARROLLADOR, y es la única que no pertenece a ningún club.

Tiene su propio panel de administración de la plataforma. Desde allí puede:

- Crear clubes. Al crear un club debe asignarle obligatoriamente un PRESIDENTE, al que se le envía
  una invitación por correo (ver §12.5).
- Modificar el escudo y los colores de cada club.
- Suspender un club, levantar su suspensión, darlo de baja, revertir la baja y eliminarlo (ver
  §7.4). Solo se puede eliminar un club que ya está dado de baja.
- Invitar a un presidente adicional a un club que ya existe.
- Quitarle el rol a un PRESIDENTE, siempre que el club conserve al menos otro ya registrado. Al
  hacerlo elige entre asignarle otro rol en ese club o eliminarlo del club por completo.

No puede nada más: no consulta ni gestiona fichas de jugadores, datos médicos, finanzas, pagos ni
información deportiva de ningún club.

El rol DESARROLLADOR no se asigna ni se retira desde la aplicación, y ningún otro rol accede al
panel de administración ni puede crear clubes. El DESARROLLADOR no crea usuarios ni asigna
contraseñas: solo envía invitaciones.

Los cuatro roles restantes pertenecen siempre a un club y su alcance nunca sale de él.

### Presidente

Es el presidente del club, que a su vez es su dueño. Administra el club completo.

Puede:

- Gestionar la configuración del club y el contenido del sitio público.
- Gestionar categorías, entrenadores y jugadores.
- Asignar entrenadores a categorías.
- Definir el valor de la mensualidad, descuentos y becas.
- Consultar las finanzas completas del club.
- Registrar, anular y corregir pagos.
- Gestionar torneos, partidos, arbitrajes y comunicados.
- Enviar invitaciones de registro a su club (ver §12.1).
- Aprobar o rechazar el ingreso de las cuentas que están en espera.
- Buscar usuarios registrados por su documento de identidad y asignarles o retirarles los roles
  ENTRENADOR o DIRECTIVO.

La asignación de roles es del PRESIDENTE, con una única excepción: un DIRECTIVO puede asignar el
rol ENTRENADOR a una cuenta cuyo ingreso acaba de aprobar (ver §12.2). Fuera de esa excepción,
ningún otro rol puede asignar, cambiar ni retirar roles, ni siquiera el suyo propio.

El rol PRESIDENTE solamente lo otorgan el DESARROLLADOR, desde su panel de administración, u otro
PRESIDENTE del mismo club (ver §12.5). Un club puede tener varios presidentes.

Cada club debe conservar siempre al menos un PRESIDENTE activo; no se permite revocar el último.

Un PRESIDENTE no puede quitarse su propio rol ni eliminar su propia cuenta mientras no exista otro
PRESIDENTE en ese club.

### Directivo

Es un miembro de la directiva del club. El rol lo asigna el PRESIDENTE.

Puede:

- Consultar las finanzas del club: recaudo, quién debe y quién está al día.
- Consultar la documentación de cada jugador.
- Consultar la seguridad social de cada jugador: su entidad de salud y dónde lo atienden.
- Enviar invitaciones de registro a su club (ver §12.1).
- Aprobar o rechazar el ingreso de cualquier cuenta que está en espera.
- Asignar el rol ENTRENADOR a una cuenta cuyo ingreso acaba de aprobar.

Su acceso a finanzas y fichas es de consulta. No puede registrar ni anular pagos, modificar el
valor de la mensualidad, asignar el rol DIRECTIVO, retirar roles ni buscar usuarios por documento.

### Entrenador

El rol lo asigna el PRESIDENTE.

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

Cada jugador tiene su propio registro de integrante, con su propio documento. Los hermanos no se
mezclan: cada uno tiene el suyo.

Una misma cuenta puede tener varios jugadores: los padres agregan a un hermano desde la ficha del
jugador ya registrado (ver §12.1.2). Quien entra con el correo elige con cuál de sus jugadores
continuar; quien entra con el documento de un jugador ve solo a ese jugador (ver §12.4). En cada
momento se ve la información de un único jugador.

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

Cada club tiene su propio sitio público, con su escudo y sus colores. No requiere inicio de sesión
y solamente muestra información de ese club.

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

El número de documento de identidad identifica de forma única a una persona dentro de un club.

Se almacena junto con su tipo:

```text
REGISTRO_CIVIL
TARJETA_IDENTIDAD
CEDULA_CIUDADANIA
CEDULA_EXTRANJERIA
```

El documento:

- Es único por pareja "documento + club": no puede repetirse dentro de un mismo club.
- Puede existir en dos clubes distintos: es la misma persona, integrante de ambos, con un único
  inicio de sesión (ver §7.3 y §12.4).
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

Cambiar la categoría de un jugador es una operación del PRESIDENTE.

Al cambiar de categoría:

- No se crea otro jugador.
- No se modifica información histórica.
- Los partidos, convocatorias, asistencias, goles y cargos anteriores conservan la categoría en la
  que ocurrieron.

No se implementará inicialmente una entidad independiente de historial de categorías del jugador.

## 12. Usuario, registro y roles

La entidad Usuario representa la cuenta de acceso.

### 12.1. Registro

El registro es por invitación. La persona recibe por correo (Brevo) un enlace de invitación de un
club; ese enlace la lleva a registrarse en ese club y la cuenta queda ligada a él (ver §7.3).

No existe un registro abierto sin invitación, y nadie elige su club al registrarse: lo determina
el enlace.

Dentro de un club, las invitaciones de registro las envían el PRESIDENTE y los DIRECTIVOS de ese
club, y nadie más. La invitación de un PRESIDENTE la envía el DESARROLLADOR (ver §12.5).

Toda invitación sirve una sola vez, caduca y queda ligada al club y al correo al que se envió: la
persona no puede registrarse con otro correo.

Quien se registra con una invitación del club pasa siempre por la sala de espera (ver §12.1.1).

- La cuenta de un jugador se registra con el documento del jugador (el niño), no con el de su
  familia.
- Un entrenador o un directivo se registra con su propio documento.

Los datos para registrarse son:

- Nombre completo del integrante del club.
- Apellidos del integrante del club.
- Correo electrónico. Para un jugador es el de su acudiente (padre, madre o responsable).
- Tipo y número de documento del integrante del club (ver §10).
- Fecha de nacimiento del integrante del club.
- Celular de contacto.
- Nombre del padre, madre o responsable, cuando el integrante es un jugador.
- Contraseña.

El correo es necesario para recibir la invitación y para recuperar la contraseña (ver §12.4).

El correo es único: no se puede registrar una cuenta nueva con un correo que ya está registrado.
Quien lo intenta recibe un mensaje que le explica que ya hay alguien registrado con ese correo.

#### 12.1.2. Hermanos: agregar un jugador desde la ficha

Un segundo jugador de la misma familia no se registra con una cuenta nueva. Sus padres entran con
la cuenta que ya tienen y, desde la ficha del hermano, agregan un nuevo jugador.

El jugador agregado:

- Comparte todos los datos de contacto de la cuenta: correo, celular y responsable.
- Tiene sus propios datos de jugador, que los padres completan: nombre, apellidos, tipo y número
  de documento, fecha de nacimiento y el resto de su ficha.
- Es un jugador independiente, con su propia ficha, categoría, cargos y pagos.
- Pasa por la sala de espera como cualquier ingreso nuevo: no tiene categoría ni genera
  mensualidad hasta que el club lo aprueba (ver §12.1.1).

Toda cuenta que se registra por sí misma recibe el rol JUGADOR y queda en espera.

#### 12.1.1. Sala de espera

Una cuenta en espera solamente ve una pantalla que le indica que su ingreso está pendiente de
aprobación.

Mientras está en espera:

- No accede a ninguna información del club: ni entrenamientos, ni horarios, ni jugadores, ni
  pagos.
- No se le asigna categoría.
- No se le genera mensualidad.

El ingreso lo aprueba el PRESIDENTE o un DIRECTIVO.

Al aprobarse el ingreso de un jugador, se le asigna la categoría que corresponde a su año de
nacimiento y empieza a generarse su mensualidad.

El PRESIDENTE o un DIRECTIVO también pueden rechazar un ingreso en espera. Rechazar borra a esa
persona del club, sin dejar datos suyos en él:

- Si pertenece a otros clubes, los conserva intactos.
- Si ese era su único club, su cuenta se elimina.
- Solo puede volver a registrarse en ese club si recibe una invitación nueva.

Solo se rechaza a quien está en espera; a un integrante ya aprobado no se le rechaza.

El estado de ingreso de una cuenta es:

```text
EN_ESPERA
APROBADO
```

El rechazo no es un estado: no queda registro del ingreso rechazado.

### 12.2. Asignación de roles

El PRESIDENTE busca a un usuario ya registrado por su documento de identidad y decide:

- Asignarle el rol ENTRENADOR.
- Asignarle el rol DIRECTIVO.
- Dejarlo como está, es decir, como JUGADOR.

El rol asignado reemplaza al rol JUGADOR con el que entró la cuenta y pasa a ser su único rol.

Cuando una cuenta nueva pasa de JUGADOR a ENTRENADOR o DIRECTIVO, la ficha de Jugador que se creó
con su registro desaparece: se elimina. Es una eliminación física justificada (ver §14), porque
esa ficha nunca correspondió a un jugador real.

Un DIRECTIVO, después de aprobar un ingreso, puede asignar a esa cuenta el rol ENTRENADOR. No
puede asignar el rol DIRECTIVO ni retirar roles.

Un rol solamente puede asignarse a un usuario que ya se registró. No se asignan roles a documentos
que aún no tienen cuenta.

La búsqueda de usuarios por documento es exclusiva del PRESIDENTE.

### 12.3. Alcance de cada rol

Los roles se resuelven mediante:

```text
Usuario
   ↓
UsuarioRol
```

Usuario no contiene CategoriaId.

El alcance de cada rol se resuelve exclusivamente así:

- DESARROLLADOR: panel de administración de la plataforma; crea clubes e invita a su PRESIDENTE
  (ver §12.5).
- PRESIDENTE: todo su club.
- DIRECTIVO: consulta de todo su club, con los límites definidos en §8.
- ENTRENADOR: mediante AsignacionEntrenadorCategoria. No se crea un UsuarioRol ENTRENADOR por cada
  categoría.
- JUGADOR: mediante Jugador.UsuarioId. La relación es uno a uno: una cuenta corresponde a un único
  jugador. Los hermanos tienen registros separados, aunque su acudiente entre con un mismo correo
  (ver §12.4).

La ficha de Jugador se crea a partir del registro de la cuenta.

### 12.4. Inicio de sesión y contraseñas

Todos los usuarios, sin importar su rol, tienen un inicio de sesión completo:

- Registro.
- Inicio de sesión.
- Olvidé mi contraseña, con recuperación por correo.
- Cambio de contraseña.

El inicio de sesión se hace con el correo o con el documento del integrante, más su contraseña.

Una persona tiene un único inicio de sesión aunque pertenezca a varios clubes; el club que ve lo
elige después, en el desplegable de su panel (ver §7.3).

Una cuenta tiene un único correo y una única contraseña, y puede tener varios jugadores (ver
§12.1.2):

- Quien entra con el correo elige, después de entrar, con cuál de sus jugadores continuar.
- Quien entra con el documento de un jugador ve solamente a ese jugador. La contraseña es la de la
  cuenta.

Una persona puede solicitar desde la aplicación la eliminación de su cuenta. La solicitud se cumple
sola, sin aprobación de nadie. Al eliminarse la cuenta:

- Sus pagos se conservan, para que el club mantenga el control de lo recaudado (ver §16.3). Junto
  a ellos solo se guardan el nombre completo y el documento.
- Se borran todos los demás datos personales.
- Se eliminan todas sus estadísticas deportivas.

Además, si se elimina el único club al que pertenece una persona, su cuenta se elimina con él (ver
§7.4).

Los correos de la cuenta se envían mediante Brevo (ver §6.3).

Tras 5 intentos fallidos seguidos de inicio de sesión, la cuenta queda bloqueada hasta que la
persona recupere su contraseña por correo.

Las contraseñas nunca se almacenan ni se envían en texto plano. Nadie asigna la contraseña de otra
persona: cada quien crea la suya al registrarse.

### 12.5. Invitación del PRESIDENTE por el DESARROLLADOR

Al crear un club, el DESARROLLADOR debe asignarle obligatoriamente un PRESIDENTE. No se puede
crear un club sin él.

Para ello indica el correo de esa persona y la plataforma le envía una invitación para registrarse
como PRESIDENTE del club creado.

Quien se registra con esa invitación:

- Completa el registro con sus propios datos y crea su propia contraseña (ver §12.1).
- Queda como PRESIDENTE de ese club, sin pasar por la sala de espera.

Un club puede tener más de un PRESIDENTE. A un presidente adicional lo elige el DESARROLLADOR,
desde su panel de administración, u otro PRESIDENTE de ese mismo club. Un PRESIDENTE lo hace
dándole el rol a alguien que ya está registrado en el club.

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

El rechazo de un ingreso en espera (ver §12.1.1) es una eliminación física justificada: esa
persona nunca llegó a entrar al club y no tiene historial en él.

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

Un pago por transferencia queda en revisión hasta que un PRESIDENTE lo confirma.

Un pago en efectivo lo registra un PRESIDENTE o el ENTRENADOR de la categoría del jugador.

Todo pago confirmado genera un recibo consultable desde la cuenta del jugador.

### 16.6. Arbitraje

El costo del arbitraje de un partido se reparte entre los jugadores convocados y genera un Cargo
de tipo ARBITRAJE para cada uno.

### 16.7. Mensualidad

La mensualidad se paga durante los primeros cinco días de cada mes.

El valor vigente en Valfor F.C. es de $65.000 COP por jugador. Es un dato de configuración de
cada club que su PRESIDENTE
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
registrados manualmente por un entrenador o por el presidente sin una regla explícita que lo autorice.

### 17.1. Tablas de posiciones

Las tablas de posiciones de los torneos son la excepción: en esta primera versión se digitan
manualmente, por torneo y categoría, por un PRESIDENTE o por el ENTRENADOR de la categoría.

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
- El presidente.

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

El sistema es para varios clubes pequeños, cada uno con decenas de jugadores, no con miles. La arquitectura debe ser
suficientemente sólida para crecer, pero no innecesariamente compleja.

## 20. Pruebas

Las reglas de negocio importantes deben tener pruebas automatizadas.

Se utilizarán:

```text
backend/pruebas/Unitarias
backend/pruebas/Integracion
```

Las pruebas deben validar principalmente:

- Generación de cargos y cálculo de saldos.
- Aplicación de pagos, abonos parciales y anulaciones.
- Idempotencia de las notificaciones de la pasarela.
- Autorización por rol.
- Que solamente el PRESIDENTE puede buscar usuarios por documento, asignar el rol DIRECTIVO y
  retirar roles.
- Que un DIRECTIVO solamente puede asignar el rol ENTRENADOR, y solo a una cuenta cuyo ingreso
  aprobó.
- Que un integrante nunca tiene más de un rol dentro de un mismo club.
- Que una persona de varios clubes solo puede elegir entre sus clubes y solo ve los datos del
  club elegido.
- Que no se puede crear un club sin asignarle un PRESIDENTE, y que quien se registra con esa
  invitación queda como PRESIDENTE de ese club.
- Que en un club suspendido solo entra su PRESIDENTE y los demás ven el aviso; que en un club dado
  de baja no entra nadie.
- Que eliminar un club borra toda su información y no afecta a ningún otro club.
- Que no es posible registrarse sin una invitación válida y que la cuenta queda en el club de la
  invitación.
- Que el mismo documento no puede repetirse en un club y sí puede existir en dos clubes.
- Que un club suspendido por impago vuelve a la normalidad al confirmarse su pago, y que un club
  suspendido conserva todos sus datos.
- Que las llaves de la pasarela de un club nunca se devuelven por la API.
- Aislamiento entre clubes: que un usuario de un club no puede ver, contar ni consultar por
  identificador ningún dato de otro club.
- Que solamente el DESARROLLADOR accede al panel de administración, crea clubes y cambia su
  escudo y colores, y que solo existe una cuenta DESARROLLADOR.
- Que el DESARROLLADOR no accede a fichas, datos médicos, finanzas ni pagos de ningún club.
- Que la ficha de Jugador se elimina cuando la cuenta pasa a ENTRENADOR o DIRECTIVO.
- Recuperación de contraseña.
- Que una cuenta en espera no accede a ninguna información del club ni genera mensualidad.
- Que solamente el PRESIDENTE o un DIRECTIVO pueden aprobar o rechazar un ingreso.
- Que solamente el PRESIDENTE o un DIRECTIVO pueden enviar invitaciones de registro a su club.
- Que una invitación no sirve dos veces, no sirve vencida y no admite otro correo.
- Que quien se registra con una invitación del club queda en espera.
- Que rechazar un ingreso borra a la persona de ese club, no afecta a sus otros clubes y le
  permite volver solo con una invitación nueva.
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

Ese lienzo define la estructura y el comportamiento de las pantallas para todos los clubes. El
escudo y los colores no son fijos: cada club se muestra con los suyos (ver §7.2). Los de Valfor
F.C. son su escudo y los colores naranja, vinotinto, dorado y negro.

El panel de administración de la plataforma no está en el lienzo; su diseño está pendiente.

Toda pantalla debe funcionar en teléfono y en escritorio.

Toda la interfaz ofrece tema claro y tema oscuro, con un botón visible para cambiar entre ellos.
Los colores de cada club deben verse bien en los dos temas. El lienzo solo muestra el tema claro.

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

- **Dirección del sitio público.** Cómo llega un visitante al sitio público de un club concreto.
- **Pago por el uso de la plataforma.** Valor, periodicidad, fecha de corte y quién lo paga dentro
  del club.
- **Sitio público de un club suspendido.** Si el sitio público de un club suspendido o dado de
  baja sigue visible.
- **Matrícula, descuentos y becas.** Si existe un cobro de inscripción y si hay descuentos (por
  ejemplo, por hermanos) o becas.
- **Fuente automática de posiciones.** Todavía no se conoce ningún enlace o servicio de las ligas
  del que se puedan leer las tablas. Mientras no exista, se mantienen manuales (ver §17.1).
- **Recordatorios de pago.** Quedan fuera de esta versión (ver §16.8). Si más adelante se quieren,
  habrá que decidir el canal.
- **Ficha con historial.** Qué pasa si una cuenta que ya tiene cargos, pagos o partidos como
  jugador pasa a ENTRENADOR o DIRECTIVO.

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

**Versión**: 3.7.0 | **Ratificada**: 2026-10-06 | **Última enmienda**: 2026-10-07
