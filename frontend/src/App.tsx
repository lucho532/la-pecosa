import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProveedorSesion } from './compartido/sesion/ProveedorSesion';
import { RutaProtegida } from './compartido/sesion/RutaProtegida';
import { ProveedorTema } from './compartido/tema/ProveedorTema';
import { Entrar } from './cuenta/Entrar';
import { Invitacion } from './cuenta/Invitacion';
import { MiPerfil } from './cuenta/MiPerfil';
import { Recuperar } from './cuenta/Recuperar';
import { Restablecer } from './cuenta/Restablecer';
import { Inicio } from './Inicio';
import { DetalleClub } from './plataforma/DetalleClub';
import { DisposicionPlataforma } from './plataforma/DisposicionPlataforma';
import { ListaClubes } from './plataforma/ListaClubes';
import { ConfiguracionClub } from './privado/ConfiguracionClub';
import { DisposicionClub } from './privado/DisposicionClub';
import { Ingresos } from './privado/ingresos/Ingresos';
import { InicioClub } from './privado/InicioClub';

/** Rutas de la aplicación: las de la tabla "Pantallas" del plan. */
export function App() {
  return (
    <ProveedorTema>
      <ProveedorSesion>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Inicio />} />
            <Route path="/entrar" element={<Entrar />} />
            <Route path="/recuperar" element={<Recuperar />} />
            <Route path="/restablecer" element={<Restablecer />} />
            <Route path="/invitacion" element={<Invitacion />} />

            <Route element={<RutaProtegida quien="cualquiera" />}>
              <Route path="/perfil" element={<MiPerfil />} />
            </Route>

            <Route element={<RutaProtegida quien="desarrollador" />}>
              <Route path="/plataforma" element={<DisposicionPlataforma />}>
                <Route index element={<ListaClubes />} />
                <Route path="clubes/:clubId" element={<DetalleClub />} />
              </Route>
            </Route>

            <Route element={<RutaProtegida quien="integrante" />}>
              <Route path="/club/:clubId" element={<DisposicionClub />}>
                <Route index element={<InicioClub />} />
                <Route path="configuracion" element={<ConfiguracionClub />} />
                <Route path="ingresos" element={<Ingresos />} />
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </ProveedorSesion>
    </ProveedorTema>
  );
}
