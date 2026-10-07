import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProveedorSesion } from './compartido/sesion/ProveedorSesion';
import { RutaProtegida } from './compartido/sesion/RutaProtegida';
import { Entrar } from './cuenta/Entrar';
import { Recuperar } from './cuenta/Recuperar';
import { Restablecer } from './cuenta/Restablecer';
import { Inicio } from './Inicio';
import { DetalleClub } from './plataforma/DetalleClub';
import { DisposicionPlataforma } from './plataforma/DisposicionPlataforma';
import { ListaClubes } from './plataforma/ListaClubes';

/** Rutas de la aplicación: las de la tabla "Pantallas" del plan. */
export function App() {
  return (
    <ProveedorSesion>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Inicio />} />
          <Route path="/entrar" element={<Entrar />} />
          <Route path="/recuperar" element={<Recuperar />} />
          <Route path="/restablecer" element={<Restablecer />} />

          <Route element={<RutaProtegida quien="desarrollador" />}>
            <Route path="/plataforma" element={<DisposicionPlataforma />}>
              <Route index element={<ListaClubes />} />
              <Route path="clubes/:clubId" element={<DetalleClub />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ProveedorSesion>
  );
}
