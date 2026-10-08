# Modelo de datos: Invitación con rol e ingreso directo al club

**Funcionalidad**: `004-invitacion-con-rol` | **Fecha**: 2026-10-08

Esta funcionalidad **no crea tablas ni columnas y no tiene migración**. Cambia qué valores toman
dos entidades que ya existen y qué transiciones se usan
([modelo de la 002](../002-ingreso-club/data-model.md),
[modelo de la 003](../003-categorias-club/data-model.md)). Los motivos están en
[research.md](research.md).

## Invitacion (cambia de uso, no de columnas)

| Campo | Antes | Ahora |
| --- | --- | --- |
| Rol | `PRESIDENTE` si la envía el DESARROLLADOR; siempre `JUGADOR` si la envía el club | `PRESIDENTE` si la envía el DESARROLLADOR; `JUGADOR`, `ENTRENADOR` o `DIRECTIVO`, el que eligió quien la envió desde el club (RF-001) |

El resto de campos, los índices, la vigencia de 7 días y los estados (pendiente, usada, vencida,
cancelada) no cambian (RF-014).

**Reglas**:

- El rol es obligatorio y no se modifica después de crear la invitación (RF-001). Reenviar crea
  otra invitación con el mismo rol; invitar de nuevo el correo anula la pendiente y crea otra con
  el rol nuevo (RF-004).
- Nunca lleva `DESARROLLADOR`. `PRESIDENTE` solo lo pone la creación de un club: el DESARROLLADOR
  ya no invita presidentes a un club que existe (RF-002, RF-024). Reenviar esa invitación sin
  usar, con el mismo correo o con uno corregido, sigue creando otra de `PRESIDENTE` (RF-025).

### Quién puede enviarla y gestionarla

| Quien actúa | Ver, invitar, reenviar y cancelar |
| --- | --- |
| PRESIDENTE | Sí, con el rol JUGADOR, ENTRENADOR o DIRECTIVO (`ReglaInvitacionDelClub`, research §2) |
| DIRECTIVO, ENTRENADOR, JUGADOR | No (RF-002, RF-003) |

Si el club tiene varios presidentes, cualquiera gestiona las invitaciones que envió otro.

## UsuarioRol (cambia de uso, no de columnas)

Cómo nace el integrante según por dónde entra:

| Entrada | Rol | EstadoIngreso | AprobadoEn, AprobadoPor…, RolDeIngreso | CategoriaId |
| --- | --- | --- | --- | --- |
| Invitación de PRESIDENTE (001) | PRESIDENTE | APROBADO | Vacíos | Nulo siempre |
| Invitación del club de JUGADOR | JUGADOR | APROBADO | Vacíos | La categoría activa de su año, o nulo si el club no la tiene (RF-011) |
| Invitación del club de ENTRENADOR o DIRECTIVO | El de la invitación | APROBADO | Vacíos | Nulo siempre (RF-012) |
| Hermano agregado desde la ficha (funcionalidad futura) | JUGADOR | EN_ESPERA | Vacíos hasta que se aprueba | Nulo hasta que se aprueba |

**Reglas**:

- El rol del integrante sale solo de la invitación; nada de lo que envía quien se registra lo
  cambia (RF-009).
- Quien entra con una invitación no tiene datos de aprobación: no es una aprobación y no aparece
  en "Ingresos aprobados" (RF-019).
- Solo el PRESIDENTE aprueba o rechaza a alguien en espera (RF-017). Al aprobar, `Rol` y
  `RolDeIngreso` quedan siempre en `JUGADOR` (RF-016). Las filas aprobadas antes de este cambio
  conservan el `RolDeIngreso` y el `AprobadoPorNombre` que tuvieran, aunque aprobara un DIRECTIVO.
- Sigue valiendo lo de la 003: solo tiene `CategoriaId` un integrante con rol JUGADOR, aprobado y
  no retirado. Como quien entra de ENTRENADOR o DIRECTIVO nunca es JUGADOR, no tiene categoría, no
  aparece en "Sin categoría" y no existe ningún dato suyo como jugador (RF-012).

### Transiciones de `EstadoIngreso`

```text
Registro o aceptación con cualquier invitación ──▶ APROBADO

Hermano agregado desde la ficha (futuro) ──▶ EN_ESPERA ──┬── aprobar ──▶ APROBADO (JUGADOR, se ubica)
                                                         └── rechazar ─▶ se borra
```

Desaparece la transición "registro con invitación del club → EN_ESPERA" de la 002. `EN_ESPERA`
sigue existiendo, pero ningún camino de esta funcionalidad lo produce.

## Usuario (cambia de uso, no de columnas)

| Campo | Antes | Ahora |
| --- | --- | --- |
| NombreResponsable | Se pedía en todo registro: obligatorio para un menor, opcional para un adulto | Solo se pide y se guarda cuando la invitación es de JUGADOR, con la misma regla de edad. En los demás roles queda nulo aunque llegue en el cuerpo (RF-013) |

## Lo que no se guarda

- Ningún historial de roles ni de invitaciones: la invitación usada, con su rol y quién la envió,
  es el único rastro de cómo entró cada persona (RF-019).
- Ninguna aprobación para quien entra con una invitación.
- Ningún dato de mensualidad: llegará con su funcionalidad (supuesto de la spec).
