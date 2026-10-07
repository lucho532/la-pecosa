import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { mensajeDe } from '../compartido/api/errores';
import type { PedirRecuperacionDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Boton } from '../compartido/componentes/Boton';
import { Campo } from '../compartido/componentes/Campo';
import { DisposicionCuenta } from './DisposicionCuenta';

/**
 * Pide el correo de recuperación de contraseña. Muestra siempre el mismo mensaje, exista o no una
 * cuenta con ese correo, para no revelarlo (RF-005).
 */
export function Recuperar() {
  const [correo, setCorreo] = useState('');
  const [enviado, setEnviado] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(evento: FormEvent) {
    evento.preventDefault();
    setError(null);
    setEnviando(true);
    try {
      const datos: PedirRecuperacionDto = { correo };
      await api.post('/api/cuenta/recuperacion', datos);
      setEnviado(true);
    } catch (fallo) {
      setError(mensajeDe(fallo));
    } finally {
      setEnviando(false);
    }
  }

  return (
    <DisposicionCuenta titulo="Recuperar contraseña">
      {enviado ? (
        <Aviso tono="exito">
          Si hay una cuenta con ese correo, te enviamos un enlace para crear una contraseña nueva. Revisa tu
          bandeja de entrada; el enlace vence en 60 minutos.
        </Aviso>
      ) : (
        <form className="columna" onSubmit={enviar}>
          <p>Escribe el correo de tu cuenta y te enviaremos un enlace para crear una contraseña nueva.</p>
          {error && <Aviso tono="error">{error}</Aviso>}
          <Campo
            etiqueta="Correo"
            type="email"
            valor={correo}
            alCambiar={setCorreo}
            autoComplete="email"
            required
          />
          <Boton type="submit" cargando={enviando} textoCargando="Enviando…">
            Enviar enlace
          </Boton>
        </form>
      )}
      <Link to="/entrar">Volver a iniciar sesión</Link>
    </DisposicionCuenta>
  );
}
