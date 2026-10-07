import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../compartido/api/cliente';
import { ErrorApi, mensajeDe } from '../compartido/api/errores';
import type { ClubDetalleDto, CrearClubDto } from '../compartido/api/tipos';
import { Aviso } from '../compartido/componentes/Aviso';
import { Boton } from '../compartido/componentes/Boton';
import { Campo } from '../compartido/componentes/Campo';
import { Tarjeta } from '../compartido/componentes/Tarjeta';

/** Códigos de conflicto que corresponden a un campo concreto del formulario. */
const CAMPO_DEL_CONFLICTO: Record<string, keyof CrearClubDto> = {
  nombre_de_club_repetido: 'nombre',
  correo_del_desarrollador: 'correoPresidente',
};

/**
 * Crea un club con el correo de su presidente, que es obligatorio. Al terminar lleva al detalle
 * del club creado.
 */
export function FormularioCrearClub({ alCancelar }: { alCancelar: () => void }) {
  const navegar = useNavigate();
  const [nombre, setNombre] = useState('');
  const [correoPresidente, setCorreoPresidente] = useState('');
  const [errores, setErrores] = useState<Partial<Record<keyof CrearClubDto, string>>>({});
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(evento: FormEvent) {
    evento.preventDefault();
    setError(null);
    setErrores({});
    setEnviando(true);
    try {
      const datos: CrearClubDto = { nombre, correoPresidente };
      const club = await api.post<ClubDetalleDto>('/api/plataforma/clubes', datos);
      navegar(`/plataforma/clubes/${club.clubId}`);
    } catch (fallo) {
      if (fallo instanceof ErrorApi && fallo.codigo === 'datos_invalidos') {
        setErrores({ nombre: fallo.errorDe('nombre'), correoPresidente: fallo.errorDe('correoPresidente') });
      } else if (fallo instanceof ErrorApi && CAMPO_DEL_CONFLICTO[fallo.codigo]) {
        setErrores({ [CAMPO_DEL_CONFLICTO[fallo.codigo]]: fallo.title });
      } else {
        setError(mensajeDe(fallo));
      }

      setEnviando(false);
    }
  }

  return (
    <Tarjeta titulo="Crear club">
      <form className="columna" onSubmit={enviar} noValidate>
        {error && <Aviso tono="error">{error}</Aviso>}
        <div className="rejilla">
          <Campo
            etiqueta="Nombre del club"
            valor={nombre}
            alCambiar={setNombre}
            error={errores.nombre}
            maxLength={120}
            required
          />
          <Campo
            etiqueta="Correo del presidente"
            type="email"
            valor={correoPresidente}
            alCambiar={setCorreoPresidente}
            error={errores.correoPresidente}
            ayuda="Le enviaremos una invitación para registrarse como presidente."
            required
          />
        </div>
        <div className="fila">
          <Boton type="submit" cargando={enviando} textoCargando="Creando…">
            Crear club e invitar
          </Boton>
          <Boton variante="secundario" onClick={alCancelar} disabled={enviando}>
            Cancelar
          </Boton>
        </div>
      </form>
    </Tarjeta>
  );
}
