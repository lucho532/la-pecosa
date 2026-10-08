using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa los datos fijos de una invitación vigente, para mostrar el registro.
/// Su responsabilidad es decir a qué club, con qué rol y a qué correo invita, si ese correo ya
/// tiene cuenta y si quien la use queda en la sala de espera.
/// No contiene el token ni ningún dato interno del club.
/// </summary>
/// <param name="NombreClub">Nombre del club que invita.</param>
/// <param name="Rol">Rol con el que entra quien la use.</param>
/// <param name="Correo">Correo invitado; no se puede cambiar.</param>
/// <param name="TieneCuenta">Si es verdadero, la persona debe iniciar sesión y aceptar la invitación.</param>
/// <param name="PasaPorSalaDeEspera">
/// Verdadero en las invitaciones del club: la pantalla no nombra ningún rol, porque se decide al
/// aprobar el ingreso.
/// </param>
/// <param name="Identidad">Identidad visual del club.</param>
public record InvitacionVigenteDto(
    string NombreClub,
    Rol Rol,
    string Correo,
    bool TieneCuenta,
    bool PasaPorSalaDeEspera,
    IdentidadClubDto Identidad);
