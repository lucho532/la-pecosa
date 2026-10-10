import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';
import './compartido/tema/variables.css';
import './compartido/tema/base.css';
import './compartido/tema/disposicion.css';
import './compartido/tema/componentes.css';
import './compartido/tema/fichas.css';
import './compartido/tema/movil.css';

createRoot(document.getElementById('raiz')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
