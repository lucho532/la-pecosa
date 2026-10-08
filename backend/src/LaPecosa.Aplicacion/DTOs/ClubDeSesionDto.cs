using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.DTOs;

/// <summary>
/// Representa un club del desplegable de la sesión.
/// Su responsabilidad es llevar lo necesario para pintar su identidad, saber el rol de la persona
/// en él y saber si su ingreso sigue en espera o si el club la retiró, para mostrarle la sala de
/// espera o el aviso de retiro sin pedir nada al club.
/// No contiene la configuración completa del club ni datos de otros integrantes.
/// </summary>
/// <param name="ClubId">Identificador del club.</param>
/// <param name="Nombre">Nombre del club.</param>
/// <param name="Rol">Rol de la persona en ese club.</param>
/// <param name="Estado">Estado actual del club.</param>
/// <param name="EstadoIngreso">Estado de ingreso de la persona en ese club.</param>
/// <param name="Retirado">Verdadero si el club retiró a este jugador; no entra a él hasta que lo reincorporen.</param>
/// <param name="Identidad">Identidad visual del club.</param>
/// <param name="Nombres">Nombres del integrante en ese club.</param>
/// <param name="Apellidos">Apellidos del integrante en ese club.</param>
public record ClubDeSesionDto(
    Guid ClubId,
    string Nombre,
    Rol Rol,
    EstadoClub Estado,
    EstadoIngreso EstadoIngreso,
    bool Retirado,
    IdentidadClubDto Identidad,
    string Nombres,
    string Apellidos);
