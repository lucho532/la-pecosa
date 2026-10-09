# Investigación: Invitación con rol e ingreso directo al club

**Funcionalidad**: `004-invitacion-con-rol` | **Fecha**: 2026-10-08

Este documento resuelve las incógnitas técnicas del plan. Las tecnologías, el aislamiento por
`ClubId`, la sesión y las pruebas son los de la 001
([research.md de la 001](../001-base-multiclub/research.md)); las invitaciones del club y la sala
de espera, los de la 002 ([research.md de la 002](../002-ingreso-club/research.md)); la ubicación
automática y el bloqueo del club, los de la 003
([research.md de la 003](../003-categorias-club/research.md)). No se repiten aquí. Cada decisión
indica qué se eligió, por qué y qué se descartó. Las marcadas con **(supuesto)** rellenan un
detalle que la spec no fija; están reunidas al final para que el propietario las confirme.

El 2026-10-08, durante este plan, el propietario decidió que dentro del club **solo el PRESIDENTE
invita y solo él aprueba o rechaza ingresos**, que el DIRECTIVO tampoco ve el apartado
"Ingresos" y que **el DESARROLLADOR solo invita al primer PRESIDENTE, al crear el club**
(constitución 4.1.0). Este documento ya lo recoge.

## 1. Dónde vive el rol de la invitación

- **Decisión**: en la columna `Invitaciones.Rol`, que ya existe desde la 001. Hasta ahora una
  invitación del club la guardaba siempre con `JUGADOR`; desde ahora guarda el rol elegido. **No
  hay migración**: esta funcionalidad no cambia el esquema de la base de datos.
- **Motivo**: la columna ya admite los cuatro roles de club y ya viaja al correo, a la consulta de
  la invitación y al registro. Solo faltaba dejar elegirlo.
- **Datos existentes**: las invitaciones del club que haya en desarrollo tienen `JUGADOR` y siguen
  sirviendo como invitaciones de JUGADOR. Quien esté `EN_ESPERA` sigue ahí y se puede aprobar o
  rechazar. No se trata nada más (RF-007 retirado).
- **Alternativas**: una columna nueva "rol de ingreso" (duplicaría la que existe).

## 2. Quién invita y con qué rol

- **Quién** (RF-002, RF-003): solo el PRESIDENTE. Los cuatro endpoints de invitaciones del club
  pasan de `[IntegranteDelClub(Rol.PRESIDENTE, Rol.DIRECTIVO)]` a
  `[IntegranteDelClub(Rol.PRESIDENTE)]`. El atributo de la 001 ya responde
  `403 rol_no_autorizado` a cualquier otro rol, también al llamar directamente; no hay código de
  autorización nuevo.
- **Con qué rol** (RF-001, RF-002): una regla de dominio pequeña, `ReglaInvitacionDelClub`, dice
  qué roles admite una invitación del club: JUGADOR, ENTRENADOR y DIRECTIVO. Ya no depende de
  quién invita.
  - Sin rol: `400 datos_invalidos`, con el error en el campo `rol` (escenario 1.5).
  - Con PRESIDENTE o DESARROLLADOR: `403 rol_no_invitable`, y no se crea ni se envía nada
    (escenario 1.4).
- **Reenviar** crea la invitación nueva con el rol de la anterior; no admite cambiarlo (RF-001,
  RF-004). **Invitar de nuevo** un correo con una invitación pendiente anula la anterior y vale el
  rol de la nueva, como hasta ahora.
- **Varios presidentes**: cualquiera de ellos ve y gestiona las invitaciones que envió otro; el
  repositorio ya lista todas las del club.
- **Lo que esta decisión ahorra** respecto de la versión anterior del plan: no hay lista de roles
  por tipo de quien invita, el servicio no necesita saber quién actúa, no existe el caso "un
  DIRECTIVO frente a una invitación de DIRECTIVO" ni su error propio.
- **Alternativas**: validar el rol con un tipo de enumeración más corto en el DTO (un rol no
  permitido llegaría como dato mal escrito, `400`, en lugar de negarse con su propio código).

## 3. Cómo entra quien usa una invitación

- **Decisión**: `ReglaIngresoPorInvitacion` deja de derivar el estado de ingreso del rol: toda
  invitación, de presidente o del club, deja a la persona `APROBADO` (RF-008). Desaparecen
  `EstadoDeIngreso` y `PasaPorSalaDeEspera`. La regla conserva lo que sí depende del rol:
  - `EsDelClub(rol)`: toda invitación que no es de PRESIDENTE. Sustituye a `PasaPorSalaDeEspera` en
    los dos usos que no tenían que ver con la espera: "una invitación del club no cambia el rol de
    quien ya es integrante" y "una invitación del club no sirve en un club dado de baja".
  - `ElClubPermiteUsarla(rol, estado)`: sin cambios de resultado (RF-021).
  - `PideResponsable(rol)`: verdadero solo para JUGADOR (decisión 5).
- **El integrante que nace**: rol de la invitación, `APROBADO`, y los cuatro datos de la
  aprobación (`AprobadoEn`, `AprobadoPorUsuarioId`, `AprobadoPorNombre`, `RolDeIngreso`) vacíos,
  exactamente como ya nace hoy un PRESIDENTE. Por eso no aparece en "Ingresos aprobados", que
  lista a quien tiene `AprobadoEn` (RF-019), y no hay que tocar esa consulta.
- **El rol nunca sale del cuerpo** (RF-009): `RegistrarConInvitacionDto` no tiene campo de rol y un
  `rol` enviado de más se ignora, como ya ocurre.
- **`EN_ESPERA`** sigue en la enumeración y en el atributo de autorización; ningún camino de esta
  funcionalidad lo produce. Las pruebas lo siembran directamente en la base de datos.

## 4. Ubicación del jugador al registrarse y aislamiento

- **Decisión**: el registro y la aceptación, cuando la invitación es de JUGADOR, ubican al jugador
  con el `UbicadorDeJugadores` de la 003, en la misma transacción que crea el integrante y con la
  fila del club bloqueada (RF-011). Con ENTRENADOR o DIRECTIVO no se llama al ubicador: el
  integrante nunca tiene categoría ni aparece en las listas de jugadores, que filtran por rol
  JUGADOR (RF-012).
- **El club de la petición**: registrarse y aceptar no llevan `clubId` en la ruta y hoy no fijan
  ningún club en `IContextoClub`, así que el filtro global no les dejaría ver categorías ni
  bloquear el club. Los dos servicios pasan a **fijar en el contexto el club de la invitación** en
  cuanto comprueban que el token es válido. A partir de ahí los repositorios del club funcionan
  igual que en una petición autorizada y limitados a ese club.
- **Por qué no rompe §7.1**: es la tercera excepción que la constitución ya admite ("abrir una
  invitación por su enlace: devuelve solo esa invitación y su club"). El token de un solo uso es lo
  que liga la petición al club, igual que el integrante lo hace en una ruta `/api/clubes/{clubId}`.
  No se añade ninguna consulta que se salte el filtro, y la respuesta no devuelve datos del club.
- **Orden dentro de la transacción**: bloquear el club, marcar la invitación como usada (sentencia
  condicionada, como hoy), crear la cuenta y el integrante, guardar y, si es JUGADOR, ubicarlo.
  Bloquear el club hace que registrarse mientras el PRESIDENTE crea la categoría del año deje
  siempre al jugador dentro de ella (caso límite de la spec), con el mismo mecanismo que la 003.
- **Alternativas**: métodos nuevos en los repositorios que reciban el `clubId` y se salten el
  filtro (más consultas sin filtro que vigilar); ubicar después, fuera de la transacción (el
  jugador podría quedar sin categoría aunque exista).

## 5. Nombre del padre, madre o responsable

- **Decisión** (RF-013): `ValidadorRegistro` recibe el rol de la invitación.
  - JUGADOR: obligatorio si es menor de 18 años el día del registro, opcional si es adulto. Como
    hoy.
  - ENTRENADOR y DIRECTIVO: no se pide. Si llega en el cuerpo se ignora y no se guarda, igual que
    un rol enviado de más; no es un error **(supuesto 2)**.
- **Invitación de PRESIDENTE** **(supuesto 1)**: hoy el registro de un presidente también muestra
  el campo. RF-013 dice "solamente cuando la invitación es de JUGADOR" y §12.1 dice "cuando el
  integrante es un jugador", así que deja de pedirse y de guardarse también ahí. Es el único efecto
  de esta funcionalidad sobre la invitación de presidente, que la spec deja fuera de alcance; por
  eso se pide confirmación.
- **La pantalla** no decide por su cuenta: `InvitacionVigenteDto` lleva `pideResponsable`, que
  sustituye a `pasaPorSalaDeEspera`.
- **Cuenta que ya existe y acepta una invitación de JUGADOR** (RF-026, decidido el 2026-10-09;
  sustituye al supuesto 3): la aceptación no pide ningún dato y la cuenta conserva el responsable
  que tenga, salvo en un caso. Si la persona es menor de 18 años ese día y su cuenta no tiene
  responsable, la aceptación lo exige y lo guarda en la cuenta; sin él responde
  `400 datos_invalidos` en `nombreResponsable` y no gasta la invitación. Puede pasar porque la
  cuenta se creó con una invitación de ENTRENADOR o DIRECTIVO, que no lo piden.
  - La edad sale de la fecha de nacimiento del integrante más reciente de la cuenta, la misma
    identidad que se copia al club nuevo (decisión 7).
  - `ReglaIngresoPorInvitacion.ExigeResponsable(rol, fechaNacimiento, hoy)` reúne la regla
    "JUGADOR y menor"; la usan el registro y la aceptación.
  - La pantalla no decide por su cuenta: `InvitacionVigenteDto` gana `faltaResponsable`, y la
    aceptación recibe `AceptarInvitacionDto` (`token` y `nombreResponsable` opcional) en lugar de
    `TokenDto`. Un responsable enviado cuando no hace falta se ignora, como en el supuesto 2.
  - **Alternativas descartadas**: rechazar la aceptación y mandar a la persona a completar su
    perfil (no existe dónde editar el responsable); dejarlo como estaba (la regla del responsable
    quedaría con una excepción que nadie ve, porque ya no hay sala de espera).

## 6. Sala de espera, aprobación y rechazo

- **Quién** (RF-017, RF-018): solo el PRESIDENTE. Los cuatro endpoints de ingresos (sala de
  espera, aprobados, aprobar y rechazar) pasan a `[IntegranteDelClub(Rol.PRESIDENTE)]`, igual que
  los de invitaciones. El DIRECTIVO tampoco consulta las listas: lo confirmó el propietario.
- **Con qué rol** (RF-016): quien se aprueba queda siempre como JUGADOR y se ubica.
- **`ReglaAprobacionIngreso` se elimina**. Sus tres preguntas ya no existen: "qué rol puede dejar
  cada quien" (siempre JUGADOR), "quién aprueba" (lo decide el atributo) y "nadie aprueba su propio
  ingreso" (quien aprueba es un PRESIDENTE y un PRESIDENTE nunca está en espera; además el
  atributo niega a quien está en espera). Mantener una regla vacía sería una abstracción sin
  necesidad (§19).
- **Contrato**: el cuerpo de la aprobación pasa a ser opcional. Sin cuerpo, sin `rol` o con
  `rol: JUGADOR` aprueba como JUGADOR; con cualquier otro rol responde `403 rol_no_asignable`, el
  código que ya existía, y la persona sigue en espera (escenario 3.3) **(supuesto 4)**. Deja de
  existir el `400` por no indicar rol.
- **Lo guardado**: `RolDeIngreso` sigue rellenándose, ahora siempre con `JUGADOR`. Las aprobaciones
  anteriores conservan el rol que tuvieran y quién las aprobó, aunque fuera un DIRECTIVO, y se
  siguen mostrando (RF-019).
- **Rechazo**: solo cambia quién puede hacerlo.
- **Alternativas**: quitar el campo `rol` del cuerpo y dejar que un `rol` enviado se ignore (la
  spec pide negarlo expresamente); quitar `RolDeIngreso` (rompería la lista de aprobados
  anteriores).

## 7. Lista de invitaciones

- **Decisión** (RF-003, RF-019): `InvitacionClubDto` gana `rol`. La lista ya muestra la invitación
  más reciente de cada correo, también las usadas, con quién la envió: con el rol añadido es donde
  el PRESIDENTE ve quién entró, con qué rol y quién lo invitó. No hace falta una lista nueva.

## 8. Correo de la invitación

- **Decisión** (RF-006): `PlantillasCorreo.Invitacion` nombra el rol en todas las invitaciones
  ("te invita a registrarte como jugador / entrenador / directivo") y desaparece la frase "el club
  revisará tu ingreso antes de darte acceso". `IServicioCorreo` no cambia de firma: ya recibía el
  rol.

## 9. Estados del club

- **Suspendido** (RF-021): el atributo de la 001 ya deja entrar solo al PRESIDENTE, que invita con
  sus tres roles. Un enlace válido sigue sirviendo y deja a la persona aprobada; al entrar ve el
  aviso de incidencia temporal, que es lo que el atributo responde a cualquier integrante que no
  es el PRESIDENTE. No hay código nuevo, solo cambia lo que las pruebas esperan (antes "queda en
  espera").
- **Dado de baja**: nadie invita (el atributo) y el enlace responde `410` (la regla de la
  decisión 3). Sin cambios.

## 10. Frontend

- **Menú del club**: el enlace "Ingresos" se pinta solo al PRESIDENTE. La pantalla, abierta por
  dirección por otro rol, muestra el error de la API, como ya hace con un ENTRENADOR.
- **"Ingresos" → Invitaciones**: el formulario gana un selector de rol obligatorio, sin valor por
  defecto, con Jugador, Entrenador y Directivo. La tabla gana la columna "Rol".
- **"Ingresos" → Sala de espera**: aprobar pasa a ser una confirmación simple con
  `DialogoConfirmacion`; se elimina `DialogoAprobarIngreso`, que solo existía para elegir rol.
  Vacía, muestra "No hay ingresos pendientes" (ya lo hace).
- **Enlace de invitación**: el subtítulo nombra siempre el rol; el formulario de registro muestra
  el responsable solo con `pideResponsable` y pierde el aviso de "quedarás pendiente de
  aprobación"; la aceptación con cuenta existente nombra el rol y pierde el mismo aviso.
- **Después de registrarse**: la navegación ya lleva al club; como el integrante llega `APROBADO`,
  `DisposicionClub` muestra la aplicación y no la pantalla de espera. `SalaDeEspera.tsx` se
  conserva para el hermano agregado desde la ficha.
- **Teléfono y temas** (RF-022): los componentes existentes (`Campo`, `Tabla`, `Tarjeta`,
  `DialogoConfirmacion`). El selector de rol es un `select` nativo.

## 11. Pruebas

Las mismas herramientas de la 001. Ningún endpoint nuevo y uno menos: `AccesoClubPruebas` pasa a
esperar 54 (decisión 12).

- **Unitarias**: `ReglaInvitacionDelClub` (nueva); `ReglaIngresoPorInvitacion`,
  `ValidadorRegistro` y `PlantillasCorreo` (cambian); se eliminan las de `ReglaAprobacionIngreso`.
- **Integración nuevas o reescritas** (§20, versión 4.1.0):
  - Solo el PRESIDENTE usa los ocho endpoints de invitaciones e ingresos: DIRECTIVO, ENTRENADOR y
    JUGADOR reciben `403 rol_no_autorizado` en todos y no cambia nada.
  - El PRESIDENTE invita con cada uno de los tres roles; con PRESIDENTE o DESARROLLADOR, `403`; sin
    rol, `400`.
  - Reenviar conserva el rol; invitar de nuevo aplica el rol nuevo; un presidente gestiona las
    invitaciones de otro.
  - Registrarse con cada rol deja ese único rol, `APROBADO`, y da acceso al club sin aprobación; la
    sala de espera queda vacía.
  - JUGADOR con categoría activa de su año queda en ella; sin ella, en "Sin categoría";
    registrarse mientras se crea la categoría termina dentro.
  - ENTRENADOR y DIRECTIVO: sin categoría, fuera de "Sin categoría" y sin responsable guardado.
  - Responsable: obligatorio para un JUGADOR menor, opcional para uno adulto, ignorado en los
    demás roles.
  - Cuenta existente que acepta: entra aprobada con el rol de la invitación y conserva su otro
    club.
  - Aprobar desde la sala de espera: siempre JUGADOR y ubicado; con otro rol, `403`.
  - Club suspendido: el presidente invita con los tres roles y quien se registra queda aprobado y
    ve `club_suspendido`.
  - El DESARROLLADOR no puede invitar a un presidente a un club que ya existe (`404`, la ruta no
    existe); crear un club sigue enviando la invitación, y reenviarla o corregir su correo sigue
    funcionando.
- **Pruebas de la 002 y la 003 que contradicen la 4.1.0** (CE-008 las excluye): las que usan a un
  DIRECTIVO para invitar, ver, aprobar o rechazar; las que esperan `EN_ESPERA` tras registrarse;
  las que eligen ENTRENADOR o DIRECTIVO al aprobar; la del `400` por aprobar sin rol y las que
  leen `pasaPorSalaDeEspera`. Se reescriben con el comportamiento nuevo. Las que necesitan a
  alguien en espera como punto de partida dejan de crearlo registrándolo y usan
  `Sembrador.CrearIntegranteEnEsperaAsync`, que ya existe.

## 12. Invitación del presidente por el DESARROLLADOR

- **Decisión** (RF-024, RF-025): se elimina `POST /api/plataforma/clubes/{clubId}/invitaciones`,
  con el método `InvitarAsync` del servicio de invitaciones de presidente, su DTO y el formulario
  "Invitar a otro presidente" del detalle del club. No queda una operación que negar: la ruta deja
  de existir.
- **Lo que se conserva**: crear un club sigue preparando y enviando la invitación de su presidente
  (misma transacción de la 001), y el reenvío con corrección de correo sigue igual. El reenvío
  solo actúa sobre una invitación sin usar, así que no sirve para sumar un presidente a un club
  que ya tiene uno registrado.
- **Contrato**: el contrato de la 004 no puede expresar que una ruta desaparece, y la prueba de
  contrato une las rutas de todas las specs. La ruta se quita del contrato de la 001, con una nota
  que remite aquí, y `AccesoClubPruebas` pasa de 55 a 54. Se hace en la misma tarea que elimina el
  endpoint, para que la prueba no quede en rojo.
- **Quitar el rol a un presidente** (001): no se toca. Exige que el club conserve otro presidente
  registrado, y hasta que exista "un PRESIDENTE elige a otro" (§12.5, sin construir) ningún club
  tendrá dos, así que en la práctica queda sin uso. Es una consecuencia de la decisión, no un
  cambio de este plan.
- **Alternativas**: conservar la ruta y responder siempre `403` (código muerto que mantener);
  dejar el cambio para otra spec (la constitución y el código quedarían en desacuerdo).

## Supuestos por confirmar

Rellenan detalles que la spec no fija. Ninguno inventa una regla de negocio nueva: cada uno aplica
al caso no previsto la regla más cercana de la spec o de la constitución (§25).

| # | Supuesto | Dónde se usa |
| --- | --- | --- |
| 1 | El nombre del responsable deja de pedirse también en el registro de un PRESIDENTE, porque RF-013 y §12.1 lo limitan al JUGADOR | Decisión 5 |
| 2 | Un `nombreResponsable` enviado con una invitación de ENTRENADOR o DIRECTIVO se ignora y no se guarda; no es un error | Decisión 5 |
| 3 | Sustituido el 2026-10-09 por RF-026: quien ya tiene cuenta y acepta una invitación de JUGADOR no ve ningún formulario, salvo el nombre del responsable si es menor de 18 años y su cuenta no lo tiene | Decisión 5 |
| 4 | La aprobación acepta que el cuerpo indique `rol: JUGADOR` o no indique nada; cualquier otro rol responde `403 rol_no_asignable` | Decisión 6 |
