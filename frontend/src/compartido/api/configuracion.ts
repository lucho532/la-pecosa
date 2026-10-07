/** Dirección base de la API. Se fija al compilar con la variable `VITE_URL_API`. */
export const URL_API: string = (import.meta.env.VITE_URL_API ?? 'http://localhost:8080').replace(/\/+$/, '');
