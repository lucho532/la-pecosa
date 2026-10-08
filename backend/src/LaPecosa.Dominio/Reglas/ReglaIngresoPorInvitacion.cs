using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa la regla que decide cómo entra al club quien usa una invitación (constitución §12.1,
/// §12.1.1 y §12.5; RF-003 y RF-014).
/// Su responsabilidad es ser el único lugar que deriva el estado de ingreso del rol de la
/// invitación: la de presidente, que envía el DESARROLLADOR, entra aprobada; cualquier otra es una
/// invitación del club y pasa siempre por la sala de espera. También dice si el estado del club
/// deja usarla.
/// No comprueba que la invitación esté vigente ni conoce HTTP ni la base de datos.
/// </summary>
public static class ReglaIngresoPorInvitacion
{
    /// <summary>Estado de ingreso con el que queda quien usa una invitación de ese rol.</summary>
    public static EstadoIngreso EstadoDeIngreso(Rol rolDeLaInvitacion) =>
        rolDeLaInvitacion == Rol.PRESIDENTE ? EstadoIngreso.APROBADO : EstadoIngreso.EN_ESPERA;

    /// <summary>Indica si quien usa una invitación de ese rol queda en la sala de espera.</summary>
    public static bool PasaPorSalaDeEspera(Rol rolDeLaInvitacion) =>
        EstadoDeIngreso(rolDeLaInvitacion) == EstadoIngreso.EN_ESPERA;

    /// <summary>
    /// Indica si el estado del club deja usar una invitación de ese rol (RF-030). Una invitación
    /// del club no sirve mientras su club está dado de baja; sí mientras está suspendido. Las de
    /// presidente no dependen del estado del club, como en la 001.
    /// </summary>
    public static bool ElClubPermiteUsarla(Rol rolDeLaInvitacion, EstadoClub estadoDelClub) =>
        !PasaPorSalaDeEspera(rolDeLaInvitacion) || estadoDelClub != EstadoClub.DADO_DE_BAJA;
}
