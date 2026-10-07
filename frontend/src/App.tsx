import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProveedorSesion } from './compartido/sesion/ProveedorSesion';
import { Entrar } from './cuenta/Entrar';
import { Recuperar } from './cuenta/Recuperar';
import { Restablecer } from './cuenta/Restablecer';
import { Inicio } from './Inicio';

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
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ProveedorSesion>
  );
}
