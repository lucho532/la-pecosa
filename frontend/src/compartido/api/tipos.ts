// Tipos de los esquemas de specs/001-base-multiclub/contracts/api.yaml, con los mismos nombres.

export type Rol = 'DESARROLLADOR' | 'PRESIDENTE' | 'DIRECTIVO' | 'ENTRENADOR' | 'JUGADOR';
export type EstadoClub = 'ACTIVO' | 'SUSPENDIDO' | 'DADO_DE_BAJA';
export type EstadoEnvio = 'PENDIENTE' | 'ENVIADO' | 'FALLIDO';
export type TipoDocumento =
  | 'REGISTRO_CIVIL'
  | 'TARJETA_IDENTIDAD'
  | 'CEDULA_CIUDADANIA'
  | 'CEDULA_EXTRANJERIA';

export interface Problema {
  title: string;
  status: number;
  codigo: string;
  errores?: Record<string, string[]>;
}

export interface TokenDto {
  token: string;
}

export interface IniciarSesionDto {
  identificador: string;
  contrasena: string;
}

export interface TokenSesionDto {
  token: string;
  venceEn: string;
}

export interface IdentidadClubDto {
  colorPrincipal: string | null;
  colorAcento: string | null;
  urlEscudo: string | null;
}

export interface ClubDeSesionDto {
  clubId: string;
  nombre: string;
  rol: Rol;
  estado: EstadoClub;
  identidad: IdentidadClubDto;
  nombres: string;
  apellidos: string;
}

export interface SesionDto {
  usuarioId: string;
  correo: string;
  esDesarrollador: boolean;
  versionFoto: number;
  clubes: ClubDeSesionDto[];
}

export interface PedirRecuperacionDto {
  correo: string;
}

export interface RestablecerContrasenaDto {
  token: string;
  contrasenaNueva: string;
}

export interface InvitacionVigenteDto {
  nombreClub: string;
  rol: Rol;
  correo: string;
  tieneCuenta: boolean;
  identidad: IdentidadClubDto;
}

export interface RegistrarConInvitacionDto {
  token: string;
  nombres: string;
  apellidos: string;
  tipoDocumento: TipoDocumento;
  numeroDocumento: string;
  fechaNacimiento: string;
  celular: string;
  contrasena: string;
}

export interface ClubDto {
  clubId: string;
  nombre: string;
  sede: string | null;
  direccion: string | null;
  correoContacto: string | null;
  telefonoContacto: string | null;
  estado: EstadoClub;
  identidad: IdentidadClubDto;
  miRol: Rol;
}

export interface ActualizarConfiguracionClubDto {
  nombre: string;
  sede: string | null;
  direccion: string | null;
  correoContacto: string | null;
  telefonoContacto: string | null;
}

export interface ClubResumenDto {
  clubId: string;
  nombre: string;
  estado: EstadoClub;
  presidenteRegistrado: boolean;
  identidad: IdentidadClubDto;
}

export interface PresidenteDto {
  usuarioRolId: string;
  nombres: string;
  apellidos: string;
  correo: string;
}

export interface InvitacionDto {
  invitacionId: string;
  correo: string;
  rol: Rol;
  estadoEnvio: EstadoEnvio;
  creadaEn: string;
  venceEn: string;
  vencida: boolean;
}

export interface ClubDetalleDto {
  clubId: string;
  nombre: string;
  sede: string | null;
  direccion: string | null;
  correoContacto: string | null;
  telefonoContacto: string | null;
  estado: EstadoClub;
  estadoCambiadoEn: string | null;
  identidad: IdentidadClubDto;
  presidentes: PresidenteDto[];
  invitaciones: InvitacionDto[];
}

export interface CrearClubDto {
  nombre: string;
  correoPresidente: string;
}

export interface ActualizarColoresDto {
  colorPrincipal: string;
  colorAcento: string;
}

export interface CambiarEstadoClubDto {
  estado: EstadoClub;
}

export interface EliminarClubDto {
  nombreDeConfirmacion: string;
}

export interface InvitarPresidenteDto {
  correo: string;
}

export interface ReenviarInvitacionDto {
  correo?: string;
}

export interface RetirarPresidenteDto {
  accion: 'ASIGNAR_ROL' | 'ELIMINAR_DEL_CLUB';
  rolNuevo?: 'DIRECTIVO' | 'ENTRENADOR';
}
