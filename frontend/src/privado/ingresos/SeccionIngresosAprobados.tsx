import type { IngresoAprobadoDto } from '../../compartido/api/tipos';
import type { Carga } from '../../compartido/api/useCarga';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Tabla, type Columna } from '../../compartido/componentes/Tabla';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { fechaYHora, nombreDeRol } from '../../compartido/formato';

const COLUMNAS: Columna<IngresoAprobadoDto>[] = [
  {
    titulo: 'Persona',
    celda: (ingreso) => (
      <strong>
        {ingreso.nombres} {ingreso.apellidos}
      </strong>
    ),
  },
  { titulo: 'Entró como', celda: (ingreso) => nombreDeRol(ingreso.rolDeIngreso) },
  { titulo: 'Aprobó', celda: (ingreso) => ingreso.aprobadoPor },
  { titulo: 'Cuándo', celda: (ingreso) => fechaYHora(ingreso.aprobadoEn) },
];

/**
 * Ingresos aprobados del club, del más reciente al más antiguo: quién entró, con qué rol, quién lo
 * aprobó y cuándo. Es de solo lectura: no ofrece ninguna acción (RF-025a).
 */
export function SeccionIngresosAprobados({ aprobados }: { aprobados: Carga<IngresoAprobadoDto[]> }) {
  return (
    <Tarjeta titulo="Ingresos aprobados">
      {aprobados.error && <Aviso tono="error">{aprobados.error}</Aviso>}
      {!aprobados.datos && aprobados.cargando && <p className="texto-suave">Cargando…</p>}
      {aprobados.datos && (
        <Tabla
          descripcion="Ingresos aprobados del club"
          columnas={COLUMNAS}
          filas={aprobados.datos}
          clave={(ingreso) => ingreso.usuarioRolId}
          vacio="Todavía no se ha aprobado ningún ingreso."
        />
      )}
    </Tarjeta>
  );
}
