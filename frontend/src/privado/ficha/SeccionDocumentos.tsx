import { useRef, useState, type ChangeEvent } from 'react';
import { api } from '../../compartido/api/cliente';
import { mensajeDe } from '../../compartido/api/errores';
import type { DocumentoDeFichaDto, DocumentoPedido, FichaJugadorDto } from '../../compartido/api/tiposFicha';
import { Aviso } from '../../compartido/componentes/Aviso';
import { Boton } from '../../compartido/componentes/Boton';
import { EtiquetaEstado } from '../../compartido/componentes/EtiquetaEstado';
import { Tabla, type Columna } from '../../compartido/componentes/Tabla';
import { Tarjeta } from '../../compartido/componentes/Tarjeta';
import { fecha } from '../../compartido/formato';
import { AYUDA_DE_ARCHIVOS, FORMATOS_ADMITIDOS, motivoParaNoEnviar } from './archivos';
import { nombreDeDocumentoPedido } from './textos';

interface Props {
  /** Dirección de la ficha en la API; los archivos cuelgan de ella. */
  rutaDeLaFicha: string;
  documentos: DocumentoDeFichaDto[];
  puedeCambiar: boolean;
  /** Recibe la ficha tal como la devolvió la API tras subir un archivo. */
  alCambiar: (ficha: FichaJugadorDto) => void;
}

/**
 * Apartado "Documentos" de la ficha: los documentos pedidos, cada uno entregado o pendiente, dicho
 * con texto además de con color. "Abrir" pide el archivo a la API con la sesión y lo muestra en
 * otra pestaña desde una dirección local, que se libera después: nunca hay una dirección pública.
 * Quien puede cambiar la ficha sube o reemplaza cada archivo; antes de enviarlo se avisa de un
 * tamaño o un formato que la API no va a admitir, pero quien decide es ella. Este apartado solo se
 * pinta si la API entregó los documentos: a un entrenador no le llegan.
 */
export function SeccionDocumentos({ rutaDeLaFicha, documentos, puedeCambiar, alCambiar }: Props) {
  const entrada = useRef<HTMLInputElement>(null);
  const [destino, setDestino] = useState<DocumentoPedido | null>(null);
  const [ocupado, setOcupado] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hecho, setHecho] = useState<string | null>(null);

  const rutaDe = (documento: DocumentoPedido) => `${rutaDeLaFicha}/documentos/${documento}`;

  async function abrir(documento: DocumentoPedido) {
    setError(null);
    setHecho(null);
    // La pestaña se abre ya, con el clic: si se abriera al llegar el archivo, el navegador la bloquearía.
    const pestana = window.open('', '_blank');
    try {
      const url = URL.createObjectURL(await api.getArchivo(rutaDe(documento)));
      if (pestana) {
        pestana.location.href = url;
      } else {
        window.location.assign(url);
      }
      window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch (fallo) {
      pestana?.close();
      setError(mensajeDe(fallo));
    }
  }

  function elegir(documento: DocumentoPedido) {
    setDestino(documento);
    entrada.current?.click();
  }

  async function subir(evento: ChangeEvent<HTMLInputElement>) {
    const archivo = evento.target.files?.[0];
    evento.target.value = '';
    if (!archivo || !destino) {
      return;
    }

    setHecho(null);
    const motivo = motivoParaNoEnviar(archivo);
    if (motivo) {
      setError(`${motivo} No se cambió el archivo que había.`);
      return;
    }

    setError(null);
    setOcupado(true);
    try {
      alCambiar(await api.putArchivo<FichaJugadorDto>(rutaDe(destino), archivo));
      setHecho(`${nombreDeDocumentoPedido(destino)}: archivo guardado.`);
    } catch (fallo) {
      setError(`${mensajeDe(fallo)} No se cambió el archivo que había.`);
    } finally {
      setOcupado(false);
    }
  }

  const columnas: Columna<DocumentoDeFichaDto>[] = [
    { titulo: 'Documento', celda: (documento) => <strong>{nombreDeDocumentoPedido(documento.documento)}</strong> },
    {
      titulo: 'Estado',
      celda: (documento) =>
        documento.entregado ? (
          <span className="fila">
            <EtiquetaEstado tono="exito" texto="Entregado" />
            {documento.subidoEn && <span>Entregado el {fecha(documento.subidoEn)}</span>}
          </span>
        ) : (
          <EtiquetaEstado tono="aviso" texto="Pendiente" />
        ),
    },
    {
      titulo: 'Acciones',
      celda: (documento) => (
        <span className="fila">
          {documento.entregado && (
            <Boton variante="secundario" onClick={() => void abrir(documento.documento)} disabled={ocupado}>
              Abrir
            </Boton>
          )}
          {puedeCambiar && (
            <Boton variante="secundario" onClick={() => elegir(documento.documento)} disabled={ocupado}>
              {documento.entregado ? 'Reemplazar' : 'Subir'}
            </Boton>
          )}
        </span>
      ),
    },
  ];

  return (
    <Tarjeta titulo="Documentos">
      {puedeCambiar && <p className="texto-suave">{AYUDA_DE_ARCHIVOS} Subir otro reemplaza al anterior.</p>}
      {error && <Aviso tono="error">{error}</Aviso>}
      {hecho && <Aviso tono="exito">{hecho}</Aviso>}
      {ocupado && <p className="texto-suave">Subiendo el archivo…</p>}
      <Tabla
        descripcion="Documentos pedidos en la ficha"
        columnas={columnas}
        filas={documentos}
        clave={(documento) => documento.documento}
        vacio="La ficha no pide ningún documento."
      />
      {puedeCambiar && (
        <input
          ref={entrada}
          type="file"
          hidden
          aria-hidden="true"
          tabIndex={-1}
          accept={FORMATOS_ADMITIDOS.join(',')}
          onChange={(evento) => void subir(evento)}
        />
      )}
    </Tarjeta>
  );
}
