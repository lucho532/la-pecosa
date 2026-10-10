# Guía de validación: Agregar un hermano y elegir el jugador

**Funcionalidad**: `006-agregar-hermano` | **Fecha**: 2026-10-09

Esta guía sirve para comprobar, de extremo a extremo, que la funcionalidad cumple la spec. No
describe cómo implementarla. El contrato está en [contracts/api.yaml](contracts/api.yaml) y el
modelo en [data-model.md](data-model.md).

## Requisitos previos

- Docker Desktop en marcha.
- SDK de .NET 10 y Node 22 (solo para ejecutar las pruebas fuera de Docker).
- La puesta en marcha es la de la [guía de la 001](../001-base-multiclub/quickstart.md):
  `docker compose up --build`, aplicación en <http://localhost:5173> y API en
  <http://localhost:8080>. Sin llave de Brevo, los enlaces de los correos se leen con
  `docker compose logs api | Select-String "Correo para"`.
- Esta funcionalidad tiene una migración (`HermanosDeLaCuenta`), que la API aplica al arrancar.
  No cambia ningún dato existente.

## Datos de partida

Con las guías de la 003, la 004 y la [005](../005-ficha-jugador/quickstart.md), dejar creados:

- **Club A**, con su presidente (presidente A), un **directivo**, un **entrenador** y la categoría
  **2014** activa. La 2016 no existe.
- En el Club A, registrados como JUGADOR: **Ana** (nacida en 2014; su cuenta es la de la familia
  de esta guía) y **Beto** (2015, otra cuenta).
- **Club B**, con otro presidente. Invitar al correo de Ana como Jugador y aceptar con su sesión:
  Ana queda como jugadora en los dos clubes.

Anotar el número de documento de Ana y tener pensados tres números nuevos para sus hermanos
**Luis** (nacido en 2014), **Mara** (2016) y **Nico** (cualquier año).

## Pruebas automatizadas

```powershell
dotnet test backend/LaPecosa.sln    # unitarias e integración (la integración necesita Docker)
npm --prefix frontend test
```

Resultado esperado: todas en verde, incluidas las de las specs anteriores sin haberlas cambiado.
La prueba de contrato espera 61 endpoints.

## Recorrido manual

Cada paso indica qué historia de la spec valida.

### 1. La familia agrega un hermano (historia 1)

1. Entrar con el **correo** de la cuenta de Ana y elegir el Club A. **Esperado**: entra
   directamente, sin lista de jugadores (todavía tiene uno solo).
2. Abrir "Mi ficha". **Esperado**: aparece "Agregar un hermano".
3. Abrir el formulario. **Esperado**: pide nombres, apellidos, tipo y número de documento y fecha
   de nacimiento; no pide correo, celular ni contraseña.
4. Confirmar con un dato vacío y después con una fecha futura. **Esperado**: no se crea y se
   explica el motivo.
5. Escribir el documento de Beto. **Esperado**: no se crea; "ese documento ya está registrado en
   el club".
6. Escribir los datos de **Luis** y confirmar. **Esperado**: confirmación de que su ingreso está
   pendiente de aprobación.
7. Agregar también a **Mara** y a **Nico**. **Esperado**: los tres quedan pendientes.
8. Entrar como presidente A y abrir una categoría y "Sin categoría". **Esperado**: Luis, Mara y
   Nico no aparecen en ninguna lista de jugadores.
9. Entrar como presidente A, directivo y entrenador y abrir la ficha de un jugador. **Esperado**:
   ninguno ve "Agregar un hermano".

### 2. La familia elige con cuál jugador continuar (historia 2)

1. Cerrar sesión y entrar con el **correo** de la cuenta de Ana, Club A. **Esperado**: antes de
   cualquier otra pantalla, la lista con Ana (activa) y Luis, Mara y Nico (pendientes de
   aprobación).
2. Elegir a Ana. **Esperado**: la aplicación del club, con el nombre de Ana a la vista; "Mi
   ficha" es la de Ana.
3. Usar "Cambiar de jugador" y elegir a Luis. **Esperado**: solo la pantalla de ingreso
   pendiente, con la opción de cambiar de jugador; ningún dato del club.
4. Con Ana elegida, copiar la dirección de su ficha, cambiar el identificador por el de Beto.
   **Esperado**: no se encuentra.
5. Recargar la página con Ana elegida. **Esperado**: sigue con Ana.
6. Cerrar sesión y volver a entrar con el correo. **Esperado**: vuelve a pedir que elija.
7. Cambiar al Club B en el desplegable. **Esperado**: entra directamente con Ana; en el Club B no
   hay hermanos ni lista.
8. Cerrar sesión y entrar con el **documento de Ana** y la contraseña de la cuenta. **Esperado**:
   entra directamente con Ana; no hay lista ni "Cambiar de jugador".
9. Cerrar sesión y entrar con el **documento de Luis** y la misma contraseña. **Esperado**: solo
   la pantalla de ingreso pendiente, sin opción de cambiar de jugador.

### 3. El PRESIDENTE aprueba o rechaza al hermano (historia 3)

1. Entrar como presidente A y abrir "Ingresos" → sala de espera. **Esperado**: Luis, Mara y Nico,
   cada uno con sus datos, el correo, el celular y el responsable de la cuenta, y "Hermano de
   Ana".
2. Aprobar a **Luis**. **Esperado**: sale de la sala de espera y aparece en la categoría 2014,
   sin equipo.
3. Aprobar a **Mara**. **Esperado**: aparece en "Sin categoría".
4. Rechazar a **Nico**. **Esperado**: desaparece de la sala de espera. En la lista de
   invitaciones sigue la invitación usada con la que entró Ana.
5. Entrar como directivo. **Esperado**: no ve "Ingresos".
6. Entrar con el correo de la cuenta de Ana. **Esperado**: la lista muestra a Ana, Luis y Mara,
   activos; Nico ya no está.
7. Elegir a Luis y abrir "Mi ficha". **Esperado**: la ficha de Luis, con su identidad, el
   contacto de la cuenta y el resto vacío, sin último cambio.
8. Escribir una alergia en la ficha de Luis, cambiar a Ana y abrir su ficha. **Esperado**: la de
   Ana no tiene esa alergia.
9. En la ficha de Luis, cambiar el celular; cambiar a Mara y abrir su ficha. **Esperado**: el
   celular nuevo (es de la cuenta).
10. Desde la ficha de Ana, agregar de nuevo a **Nico** con el mismo documento. **Esperado**: se
    acepta y vuelve a quedar pendiente.
11. Como presidente A, retirar a Luis. Entrar con el correo de la cuenta de Ana. **Esperado**:
    Luis figura como retirado; al elegirlo solo se ve el aviso de retiro y la opción de cambiar;
    Ana y Mara siguen igual.

### 4. Comprobaciones transversales

1. Con una cuenta de un solo jugador (Beto), recorrer el inicio, "Mi categoría" y "Mi ficha".
   **Esperado**: todo igual que antes de esta funcionalidad, sin lista ni "Cambiar de jugador".
2. Repetir los pasos 1.3, 2.1 y 3.1 a 360 px de ancho y en tema claro y oscuro. **Esperado**: sin
   desplazamiento horizontal y con el texto legible.
3. Comparar Swagger (<http://localhost:8080/swagger>) con [contracts/api.yaml](contracts/api.yaml).
   **Esperado**: el endpoint de hermanos existe, y la sesión y la sala de espera traen los campos
   nuevos.

## Comprobaciones directas contra la API

Para lo que la pantalla no deja intentar. `$t` es el token de la cuenta de Ana obtenido con el
correo; `$ana` y `$luis`, sus identificadores (vienen en `GET /api/sesion`).

| Petición | Esperado |
| --- | --- |
| `GET /api/clubes/{A}` sin cabecera | `409 jugador_sin_elegir` |
| La misma, con `X-Jugador-Elegido: $ana` | `200` |
| La misma, con la cabecera de Beto | `404` |
| `GET …/jugadores/$luis/ficha` con la cabecera de Ana | `404` |
| `POST …/jugadores/$luis/hermanos` con la cabecera de Ana | `404` |
| `GET /api/sesion` con un token obtenido con el documento de Ana | El Club A trae `jugadores` vacía y los datos de Ana |
| `GET …/jugadores/$luis/ficha` con ese token y la cabecera de Luis | `404` |
| `POST …/hermanos` como presidente A | `403 rol_no_autorizado` |
| `POST …/hermanos` dos veces con el mismo cuerpo | `201` y después `200`, con el mismo jugador |
