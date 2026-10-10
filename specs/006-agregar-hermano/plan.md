# Plan de implementación: Agregar un hermano y elegir el jugador

**Rama**: `006-agregar-hermano` | **Fecha**: 2026-10-09 | **Spec**: [spec.md](spec.md)

**Entrada**: especificación de la funcionalidad en `specs/006-agregar-hermano/spec.md`

## Resumen

La familia que ya tiene un hijo en el club agrega a otro desde "Mi ficha": escribe su identidad y
el nuevo jugador queda en la sala de espera, con el correo, el celular, el responsable y la
contraseña de la cuenta. El PRESIDENTE lo aprueba o lo rechaza con la sala de espera que ya
existe, y ve de qué jugador es hermano. Cuando la cuenta tiene varios jugadores en el club, quien
entra con el correo elige con cuál continuar y puede cambiar sin cerrar sesión; quien entra con el
documento de uno ve solo a ese.

Enfoque técnico: un hermano es otra fila de `UsuarioRol` con la misma cuenta y el mismo club; no
hay tablas nuevas y la migración añade una sola columna, el jugador de origen. El cambio de fondo
está en un único punto: `IntegranteDelClubAttribute` deja de suponer un integrante por cuenta y
club y pregunta a una regla de dominio nueva, `ReglaJugadorDeLaSesion`, cuál es el de la petición.
El elegido viaja en la cabecera `X-Jugador-Elegido`; la sesión iniciada con documento lleva en el
token los jugadores a los que está limitada. Como todos los servicios reciben ya "quien pregunta"
como un `UsuarioRol`, quedan atados al jugador elegido sin cambiarlos. Un endpoint nuevo para
agregar al hermano; la sesión pasa a devolver una entrada por club con sus jugadores. Las
decisiones y sus alternativas están en [research.md](research.md).

## Contexto técnico

**Lenguaje y versión**: C# 14 sobre .NET 10 en el backend; TypeScript 5 sobre Node 22 en el
frontend. Sin cambios respecto de la 001.

**Dependencias principales**: las de la 001 (ASP.NET Core 10, Entity Framework Core 10 con Npgsql,
JWT, React 19, React Router, Vite). No se añade ninguna.

**Almacenamiento**: PostgreSQL 17. Una migración, `HermanosDeLaCuenta`, con una columna nueva en
`UsuariosRol`. La elección de jugador no se guarda en el servidor.

**Pruebas**: xUnit (unitarias e integración con `WebApplicationFactory` y Testcontainers); Vitest
en el frontend.

**Plataforma de destino**: la de la 001: API en contenedor Linux y aplicación web adaptable.

**Tipo de proyecto**: aplicación web con `backend/` y `frontend/` separados (§4).

**Objetivos de rendimiento**: los de la 001. La autorización de cada petición sigue haciendo una
sola consulta de integrantes; ahora trae todos los de la cuenta en el club, que son unos pocos.

**Restricciones**: código y documentación en español (§2.1, §3); ningún archivo escrito a mano por
encima de 250 líneas (§2.3); toda autorización y todo aislamiento en el servidor (§15, RF-031);
backend y pantalla de cada historia en la misma tarea (§27.1); una cuenta con un solo integrante
debe comportarse exactamente igual que hoy, sin enviar nada nuevo (§27.2).

**Escala y alcance**: decenas de jugadores por club y dos o tres hermanos por cuenta (§19). Esta
funcionalidad tiene 0 entidades y 1 regla de dominio nuevas; 1 operación de API nueva y 6 que
cambian; 3 componentes de pantalla nuevos y 7 archivos de frontend que cambian.

## Comprobación de la constitución

*Puerta: debe pasar antes de la fase 0 y se revisa de nuevo tras el diseño de la fase 1.*

Evaluada contra la versión **4.1.0**.

| Principio | Cómo lo cumple el plan | Resultado |
| --- | --- | --- |
| §2.1 y §2.2 Español y vocabulario | `ReglaJugadorDeLaSesion`, `ServicioAgregarHermano`, `X-Jugador-Elegido`; "hermano" y "jugador", sin entidad Familia ni Acudiente | Cumple |
| §2.3 Máximo 250 líneas | Lo nuevo va en archivos propios; `tipos.ts` (243 líneas) no crece: los tipos nuevos van en `tiposSesion.ts` | Cumple |
| §3 Documentación XML | En cada clase nueva, y se reescribe en las que cambian de responsabilidad (`IntegranteDelClubAttribute`, `ValidadorSesion`, `IEmisorTokenSesion`, `AccesoAFicha`, `ClubDeSesionDto`, `UsuarioRol`) | Cumple |
| §4 y §5 Capas y dependencias | La decisión de quién hace la petición es una regla de `Dominio/Reglas`; el atributo solo la consulta; el controlador nuevo solo delega | Cumple |
| §6 Tecnologías base | Ninguna nueva | Cumple |
| §7.1 Aislamiento entre clubes | La cabecera solo vale para un integrante de la cuenta **en el club de la ruta**; la lectura de integrantes de la cuenta filtra por club | Cumple |
| §7.3 Varios clubes | Un inicio de sesión; el club se sigue eligiendo en el desplegable; el hermano se agrega solo en el club de la ficha | Cumple |
| §7.5 Aislamiento dentro del club | La cuenta solo llega a sus jugadores; con uno elegido, tampoco a la ficha del hermano; conocer un identificador no da acceso | Cumple |
| §8 Jugador | Varios jugadores por cuenta; en cada momento, la información de uno solo | Cumple |
| §10 Documento | Único por club, en cualquier estado; el índice único ya existe | Cumple |
| §12.1.1 Sala de espera | Solo recibe al hermano; en espera no accede a nada del club, sin categoría; aprueba y rechaza solo el PRESIDENTE; rechazar borra sin dejar datos y deja intacta la cuenta | Cumple |
| §12.1.2 Hermanos | Se agrega desde la ficha, comparte el contacto de la cuenta, es un jugador independiente y pasa por la sala de espera | Cumple |
| §12.3 Alcance del JUGADOR | "Una cuenta puede tener varios jugadores… y solamente accede a los suyos". El jugador sigue siendo el `UsuarioRol` | Cumple, con la desviación heredada (Seguimiento de complejidad) |
| §12.4 Inicio de sesión | Con correo se elige después de entrar; con documento se ve solo a ese jugador; la contraseña es la de la cuenta | Cumple |
| §13 Histórico | La aprobación sigue copiando el nombre de quien aprueba; el jugador de origen no es histórico y no se copia | Cumple |
| §14.1 Retiro | Es de cada jugador; retirar a uno no toca a sus hermanos | Cumple |
| §15 Reglas en backend | Quién hace la petición, la limitación por documento y todas las validaciones se deciden en la API | Cumple |
| §18 Estados | `EN_ESPERA` y `APROBADO`, sin estado de rechazo | Cumple |
| §19 Simplicidad | Una columna, una regla, una cabecera; sin tablas, sin sesiones guardadas, sin entidad nueva | Cumple |
| §20 Pruebas | Autorización por rol, aislamiento entre cuentas, entre hermanos y entre clubes, sesión limitada por documento (research §11) | Cumple |
| §22 Migraciones | Una migración de Entity Framework | Cumple |
| §23 API | HTTP semántico, DTOs, `problem+json`, Swagger | Cumple |
| §24 Diseño visual | Componentes y temas existentes; el estado de cada jugador, con texto además de color | Cumple, con la nota 1 |
| §25 Decisiones no tomadas | Cinco detalles que la spec no fija quedan como supuestos en research.md | Cumple, con supuestos por confirmar |
| §27 Definición de terminado | Tareas por historia, cada una con backend, pantalla y pruebas | Cumple |
| §28 Decisiones pendientes | No se resuelve ninguna | Cumple |

**Resultado de la puerta**: pasa, con la misma desviación de la letra de §12.3 que ya justificaron
la 003, la 004 y la 005.

**Nota 1**: no he podido abrir el lienzo de §24 desde esta sesión. Las pantallas se componen con
los componentes ya aprobados; si el lienzo trae una pantalla de elección de jugador, se contrasta
con ella al construir la historia 2.

**Revisión tras el diseño**: el modelo de datos y el contrato no añaden excepciones al aislamiento,
tecnologías ni patrones respecto de lo evaluado. La cabecera no concede nada que la cuenta no
tenga, y la sesión con documento no recibe ningún dato de un hermano, tampoco en `GET /api/sesion`.
La puerta sigue pasando.

## Estructura del proyecto

### Documentación de esta funcionalidad

```text
specs/006-agregar-hermano/
├── plan.md              # Este archivo
├── research.md          # Decisiones técnicas y supuestos del plan
├── data-model.md        # La columna nueva y cómo nace un hermano
├── quickstart.md        # Guía de validación de extremo a extremo
├── contracts/
│   └── api.yaml         # Contrato OpenAPI: un endpoint nuevo y seis que cambian
└── tasks.md             # Lo genera /speckit-tasks
```

### Código fuente

Solo se listan los archivos nuevos (`+`) y los que cambian (`~`). La estructura es la de la 001.

```text
backend/src/
├── LaPecosa.Dominio/
│   ├── Entidades/
│   │   └── ~ UsuarioRol.cs                  # AgregadoDesdeUsuarioRolId y su navegación
│   └── Reglas/
│       ├── + ReglaJugadorDeLaSesion.cs      # cuál integrante hace la petición
│       ├── + JugadorDeLaPeticion.cs         # el resultado: integrante, no encontrado o sin elegir
│       └── ~ ReglaAccesoAFicha.cs           # solo el nombre del parámetro: "es el de la petición"
├── LaPecosa.Aplicacion/
│   ├── DTOs/              + AgregarHermanoDto, JugadorDeSesionDto
│   │                      ~ ClubDeSesionDto (UsuarioRolId, Jugadores),
│   │                        IngresoEnEsperaDto (HermanoDe)
│   ├── Interfaces/        ~ IRepositorioPertenencias  # integrantes de la cuenta en un club;
│   │                                                  # identificadores por documento
│   │                      ~ IEmisorTokenSesion        # admite la limitación
│   ├── Servicios/         + IServicioAgregarHermano
│   │                      ~ IServicioSesion           # la limitación entra en obtener y renovar
│   ├── Implementaciones/  + ServicioAgregarHermano
│   │                      ~ ServicioSesion, ServicioRechazoIngreso
│   ├── Mappers/           ~ MapperSesion (agrupa por club), MapperIngresos (hermanoDe)
│   ├── Validadores/       + ValidadorHermano          # ValidadorIdentidad más el responsable
│   └── Utilidades/        ~ AccesoAFicha              # compara con el jugador de la petición
│                          + ErroresDeSesion           # jugador_sin_elegir
├── LaPecosa.Infraestructura/
│   ├── Datos/
│   │   ├── Configuraciones/  ~ ConfiguracionUsuarioRol
│   │   └── Migraciones/      + HermanosDeLaCuenta (generada)
│   ├── Repositorios/      ~ RepositorioPertenencias, RepositorioIngresos (incluye el origen)
│   └── Seguridad/         ~ EmisorTokenSesion, OpcionesSesion (reclamación `jugadores`)
└── LaPecosa.Api/
    ├── Autorizacion/      ~ IntegranteDelClubAttribute   # lee la cabecera y aplica la regla
    │                      ~ ValidadorSesion              # deja la limitación en la petición
    ├── Configuracion/     ~ ConfiguracionSeguridad (CORS admite la cabecera),
    │                        RegistroDeCasosDeUso
    └── Controladores/
        ├── ~ ControladorBase.cs             # expone la limitación de la sesión
        ├── Cuenta/        ~ ControladorSesion
        └── Club/          + ControladorHermanos

backend/pruebas/
├── Unitarias/
│   ├── Reglas/            + ReglaJugadorDeLaSesionPruebas
│   │                      ~ ReglaAccesoAFichaPruebas (nombre del parámetro)
│   └── Validadores/       + ValidadorHermanoPruebas
└── Integracion/
    ├── Base/              ~ ClienteDePrueba          # enviar la cabecera
    │                      + SembradorHermanos        # cuenta con dos jugadores, uno en espera
    ├── Hermanos/          + EscenarioHermanos
    │                      + AgregarHermanoPruebas    # crea, comparte contacto, queda en espera
    │                      + AgregarHermanoRechazosPruebas  # documento, validación, responsable, doble
    │                      + QuienAgregaPruebas       # roles, retirado, en espera, hermano ajeno
    │                      + HermanoEnEsperaPruebas   # nada del club, fuera de las listas
    │                      + ElegirJugadorPruebas     # sesión por club, cabecera, sin elegir
    │                      + SesionConDocumentoPruebas  # limitada, renovación, cambio de documento
    │                      + AprobarHermanoPruebas    # categoría del año, ficha propia y vacía
    │                      + RechazarHermanoPruebas   # borra solo a ese; conserva la invitación
    │                      + FichaEntreHermanosPruebas  # RF-030
    │                      + HermanosEntreClubesPruebas
    ├── Ingresos/          ~ SalaDeEsperaPruebas (hermanoDe)
    └── Aislamiento/       ~ AccesoClubPruebas        # 61 endpoints en los contratos

frontend/src/
├── compartido/
│   ├── api/               + tiposSesion.ts           # JugadorDeSesionDto, AgregarHermanoDto
│   │                      ~ tipos.ts (ClubDeSesionDto, IngresoEnEsperaDto), cliente.ts (cabecera)
│   └── sesion/            + jugadorElegido.ts        # la elección por club, en sessionStorage
│                          ~ ProveedorSesion.tsx      # la borra al iniciar y al cerrar
├── privado/
│   ├── ~ DisposicionClub.tsx                 # lista antes que nada; nombre del jugador; cambiar
│   ├── + ElegirJugador.tsx                   # la lista de jugadores con su estado
│   ├── + BotonCambiarJugador.tsx
│   ├── ~ SalaDeEspera.tsx, AvisoRetirado.tsx # ofrecen cambiar de jugador
│   ├── ficha/
│   │   ├── ~ FichaJugador.tsx                # "Agregar un hermano", solo en la ficha propia
│   │   └── + DialogoAgregarHermano.tsx
│   └── ingresos/          ~ SeccionSalaDeEspera.tsx  # "Hermano de …"
└── pruebas/               + jugadorElegido.test.ts
```

**Decisión de estructura**: la de la 001, sin carpetas nuevas salvo `pruebas/Integracion/Hermanos/`.
`DisposicionClub.tsx` tiene 141 líneas; la lista y el botón van en componentes propios para que no
pase de 250.

### Pantallas

| Ruta | Qué cambia | Quién |
| --- | --- | --- |
| `/club/:clubId/**` | Si la cuenta tiene varios jugadores y no hay elección, se muestra la lista antes que cualquier otra cosa | JUGADOR que entró con el correo |
| Menú del club | El nombre que ya se muestra es el del jugador elegido; "Cambiar de jugador" | JUGADOR con varios jugadores |
| Pantalla de ingreso pendiente y aviso de retiro | "Cambiar de jugador" | JUGADOR con varios jugadores |
| `/club/:clubId/jugadores/:usuarioRolId/ficha` | Botón "Agregar un hermano" y su diálogo, solo en la ficha propia | JUGADOR |
| `/club/:clubId/ingresos` → Sala de espera | "Hermano de …" en cada ingreso | PRESIDENTE |

## Trazabilidad

| Historia | Requisitos | Backend | Pantallas | Pruebas clave |
| --- | --- | --- | --- | --- |
| 1. La familia agrega un hermano | RF-001 a 012, 014, 032 | `ServicioAgregarHermano`, `ValidadorHermano`, `ControladorHermanos`, columna de origen | Botón y diálogo en la ficha | Crea en espera con el contacto de la cuenta; sin categoría y fuera de las listas; documento repetido `409`; validación; responsable; doble confirmación; roles `403`; desde un hermano `404` |
| 2. La familia elige con cuál continuar | RF-013, 021 a 031 | `ReglaJugadorDeLaSesion`, `IntegranteDelClubAttribute`, `ServicioSesion`, `EmisorTokenSesion`, `MapperSesion`, `AccesoAFicha` | Lista de jugadores, cambiar de jugador, pendiente y retirado | Una entrada por club con `jugadores`; uno solo, lista vacía; sin cabecera `409`; ajeno `404`; en espera y retirado `403`; con documento, limitada y sin hermanos en la sesión; ficha del hermano `404` |
| 3. El PRESIDENTE aprueba o rechaza | RF-015 a 020 | `MapperIngresos`, `RepositorioIngresos`, `ServicioRechazoIngreso` | "Hermano de …" en la sala de espera | `hermanoDe`; aprobar ubica por año y da ficha vacía; rechazar borra solo a ese y conserva la invitación del primero; volver a agregarlo; otros roles `403` |
| Transversal | RF-033 | Estado del club, dos clubes | Todas, a 360 px y en los dos temas | Hermano solo en el club de origen; club suspendido; el contrato pasa a 61 endpoints; la 001 a la 005 en verde |

## Orden de construcción sugerido

Para `/speckit-tasks`. Cada bloque deja algo que se puede probar de extremo a extremo (§27.1).

1. **Base**: la columna y su migración; `ReglaJugadorDeLaSesion` con sus pruebas unitarias; la
   lectura de integrantes de la cuenta en un club; `IntegranteDelClubAttribute` con la cabecera y
   CORS; `AccesoAFicha` por jugador de la petición. Al terminar, todas las pruebas anteriores
   siguen en verde y un sembrador de pruebas ya puede crear cuentas con dos jugadores.
2. **Historia 2**: la sesión agrupada por club con `jugadores`; la limitación por documento en el
   token, la validación y la renovación; la lista de jugadores, el cambio de jugador y las
   pantallas de pendiente y retirado. Se prueba con hermanos sembrados.
3. **Historia 1**: el endpoint de agregar con su validador; el botón y el diálogo en la ficha.
4. **Historia 3**: `hermanoDe` en la sala de espera; el cambio del rechazo; pruebas de aprobar y
   rechazar a un hermano.
5. **Cierre**: dos clubes, estados del club, revisión a 360 px en los dos temas, contrato frente a
   Swagger, documentación XML de las clases que cambiaron de responsabilidad y recorrido del
   quickstart.

La historia 2 va antes que la 1 aunque las dos sean P1: sin elegir jugador, un hermano recién
agregado deja a la cuenta sin poder entrar al club (`409 jugador_sin_elegir`). Ninguna de las dos
debe publicarse sin la otra.

## Cambios sobre lo ya construido en la 001, la 002, la 004 y la 005

Son los puntos donde hay que cuidar la no regresión (§27.2):

| Qué cambia | Por qué | Cómo se protege |
| --- | --- | --- |
| `IntegranteDelClubAttribute` lee todos los integrantes de la cuenta en el club y aplica la regla | RF-021 a 029 | Con un solo integrante y sin cabecera el resultado es el de hoy; todas las pruebas de integración anteriores pasan por aquí sin cambios |
| El token puede llevar la reclamación `jugadores` | RF-026 | Un token sin ella se comporta como hoy; las pruebas de sesión de la 001 que entran con documento usan cuentas de un integrante y siguen en verde |
| `GET /api/sesion` devuelve una entrada por club | Con hermanos habría dos del mismo club | Para cuentas de un integrante por club la lista es idéntica; `ClubDeSesionDto` solo gana campos |
| La ficha compara con el jugador de la petición y no con la cuenta | RF-030 | Con un jugador por cuenta es lo mismo; `FichaAjenaPruebas` sigue en verde; prueba nueva entre hermanos |
| El rechazo conserva las invitaciones si a la cuenta le queda otro integrante en el club | La invitación usada del primer hijo es el registro de su ingreso (004) | `RechazoPruebas` no cambia: sin hermanos se siguen borrando; prueba nueva |
| `IngresoEnEsperaDto` gana `hermanoDe` | RF-015 | Campo añadido; `SalaDeEsperaPruebas` lo comprueba |
| `cliente.ts` añade una cabecera a las peticiones del club | RF-021 | Solo cuando hay elección; sin ella, las peticiones son las de hoy |
| `AccesoClubPruebas` espera 61 endpoints en lugar de 60 | Un endpoint nuevo | El contrato de la 006 se añade en la misma tarea que el endpoint |

## Seguimiento de complejidad

| Desviación | Por qué hace falta | Alternativa más simple descartada |
| --- | --- | --- |
| §12.3 escribe `Jugador.UsuarioId`; el jugador sigue siendo el `UsuarioRol` con rol JUGADOR | Heredada de la 003, la 004 y la 005. El `UsuarioRol` ya cumple lo que §12.3 pide del jugador (pertenece a una única cuenta y tiene su propio registro); varios por cuenta no exigen la entidad | Dejarlo como está **es** la opción simple. La entidad `Jugador` completa se descarta por §19 y §27.2; si el propietario la quiere, es una spec de refactorización propia |

## Consecuencias que conviene tener presentes

- **Cuenta con varios jugadores que recibe una invitación de PRESIDENTE**: la aceptación de la 001
  convierte en PRESIDENTE al integrante más antiguo y los demás siguen como JUGADOR. La cuenta
  queda con dos roles en el club y la lista de elección los mostraría juntos. Este plan no lo
  cambia: es el caso que §28 deja abierto ("Ficha con historial") y no debe resolverse aquí.
- **La cabecera es obligatoria para las cuentas con hermanos**: cualquier cliente futuro de la API
  (por ejemplo la aplicación móvil de §6.1) tendrá que enviarla.
- **Sesiones abiertas al desplegar**: un token anterior no lleva limitación. Quien había entrado
  con un documento y después agrega un hermano tendrá, hasta que vuelva a entrar, una sesión que
  elige como si hubiera entrado con el correo.
- **Entrar con el documento de un hermano en espera** funciona y muestra solo la pantalla de
  pendiente, como pide la spec.

## Supuestos del plan

Detallados al final de [research.md](research.md). Conviene que el propietario los confirme antes
de `/speckit-tasks` o durante `/speckit-analyze`:

1. No se admite el documento de un hermano que ya usa otra cuenta en otro club, como en el
   registro y en el cambio de documento.
2. Confirmar dos veces el mismo hermano devuelve el que ya está en espera, sin error.
3. La elección de jugador sobrevive a recargar la página y se pierde al cerrar la pestaña, al
   cerrar sesión y al volver a entrar.
4. Si la cuenta ya tiene responsable, el que se escriba al agregar un hermano se ignora.
5. Si el jugador de origen deja de existir, la sala de espera no dice de quién es hermano.
