import { temaContrario } from '../tema/tema';
import { useTema } from '../tema/useTema';
import { Boton } from './Boton';

const NOMBRE = { claro: 'claro', oscuro: 'oscuro' } as const;

/**
 * Botón visible para cambiar entre tema claro y oscuro (RF-032). Su texto dice a qué tema cambia,
 * así que se entiende sin depender de un icono ni del color.
 */
export function BotonTema({ enLateral = false }: { enLateral?: boolean }) {
  const { tema, alternar } = useTema();

  return (
    <Boton variante={enLateral ? 'lateral' : 'secundario'} onClick={alternar}>
      Cambiar a tema {NOMBRE[temaContrario(tema)]}
    </Boton>
  );
}
