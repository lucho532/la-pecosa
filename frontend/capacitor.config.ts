import type { CapacitorConfig } from '@capacitor/cli';

/*
 * Empaquetado Android de la misma aplicación React (constitución §6.1). El identificador usa
 * `lapecosa` porque no admite guion (§1).
 *
 * Con la variable CAP_SERVIDOR_DESARROLLO (por ejemplo http://localhost:5173) el APK no lleva la
 * web dentro: carga el servidor de Vite del ordenador, que llega al teléfono por USB con
 * `adb reverse`. Sirve para ver la vista móvil mientras se desarrolla. Sin ella, la web va dentro
 * del APK, que es lo que se publicará.
 */
const servidorDesarrollo = process.env.CAP_SERVIDOR_DESARROLLO;

const config: CapacitorConfig = {
  appId: 'com.lapecosa.app',
  appName: 'La Pecosa',
  webDir: 'dist',
  ...(servidorDesarrollo ? { server: { url: servidorDesarrollo, cleartext: true } } : {}),
};

export default config;
