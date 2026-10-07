import { useState, type FormEvent } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../compartido/api/errores';
import type { RestablecerContrasenaDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Boton } from '../compartido/componentes/Boton';
import { Campo } from '../compartido/componentes/Campo';
import { DisposicionCuenta } from './DisposicionCuenta';

/**
 * Crea una contraseña nueva con el enlace del correo. El token viene en el fragmento de la
 * dirección (`/restablecer#<token>`) y se envía a la API en el cuerpo.
 */
export function Restablecer() {
  const token = useLocation().hash.replace(/^#/, '');
  const [contrasena, setContrasena] = useState('');
  const [repetida, setRepetida] = useState('');
  const [errorCampo, setErrorCampo] = useState<string | undefined>();
  const [error, setError] = useState<string | null>(null);
  const [enlaceMuerto, setEnlaceMuerto] = useState(token.length === 0);
  const [hecho, setHecho] = useState(false);
  const [enviando, setEnviando] = useState(false);

  async function enviar(evento: FormEvent) {
    evento.preventDefault();
    setError(null);
    setErrorCampo(undefined);
    if (contrasena !== repetida) {
      setErrorCampo('Las dos contraseñas no coinciden.');
      return;
    }

    setEnviando(true);
    try {
      const datos: RestablecerContrasenaDto = { token, contrasenaNueva: contrasena };
      await api.post('/api/cuenta/recuperacion/confirmacion', datos);
      setHecho(true);
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.codigo === 'enlace_no_valido') {
        setEnlaceMuerto(true);
      } else if (fallo instanceof ErrorApi && fallo.errorDe('contrasenaNueva')) {
        setErrorCampo(fallo.errorDe('contrasenaNueva'));
      } else {
        setError(mensajeDe(fallo));
      }
    } finally {
      setEnviando(false);
    }
  }

  if (hecho) {
    return (
      <DisposicionCuenta titulo="Contraseña creada">
        <Aviso tono="exito">Tu contraseña nueva ya está lista.</Aviso>
        <Link to="/entrar">Iniciar sesión</Link>
      </DisposicionCuenta>
    );
  }

  if (enlaceMuerto) {
    return (
      <DisposicionCuenta titulo="Enlace no válido">
        <Aviso tono="aviso">Este enlace ya se usó o caducó.</Aviso>
        <Link to="/recuperar">Pedir un enlace nuevo</Link>
      </DisposicionCuenta>
    );
  }

  return (
    <DisposicionCuenta titulo="Crear contraseña">
      <form className="columna" onSubmit={enviar}>
        {error && <Aviso tono="error">{error}</Aviso>}
        <Campo
          etiqueta="Contraseña nueva"
          type="password"
          valor={contrasena}
          alCambiar={setContrasena}
          autoComplete="new-password"
          ayuda="Entre 8 y 128 caracteres."
          error={errorCampo}
          required
        />
        <Campo
          etiqueta="Repite la contraseña"
          type="password"
          valor={repetida}
          alCambiar={setRepetida}
          autoComplete="new-password"
          required
        />
        <Boton type="submit" cargando={enviando} textoCargando="Guardando…">
          Guardar contraseña
        </Boton>
      </form>
    </DisposicionCuenta>
  );
}
