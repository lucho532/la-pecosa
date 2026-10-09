using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa la regla de lo que depende del rol de una invitación al usarla (constitución §12.1 y
/// §12.5; RF-013 y RF-021).
/// Su responsabilidad es distinguir la invitación del club de la de presidente, decir si el
/// registro pide el nombre del responsable y si el estado del club deja usarla.
/// No decide el estado de ingreso: quien usa una invitación, del rol que sea, entra siempre
/// aprobado y sin sala de espera (RF-008). No comprueba que la invitación esté vigente ni conoce
/// HTTP ni la base de datos.
/// </summary>
public static class ReglaIngresoPorInvitacion
{
    /// <summary>
    /// Indica si la invitación de ese rol la envió el club: toda la que no es de PRESIDENTE. Una
    /// invitación del club no cambia a quien ya es integrante y no sirve en un club dado de baja.
    /// </summary>
    public static bool EsDelClub(Rol rolDeLaInvitacion) => rolDeLaInvitacion != Rol.PRESIDENTE;

    /// <summary>
    /// Indica si el registro con una invitación de ese rol pide el nombre del padre, madre o
    /// responsable: solo la de JUGADOR (RF-013).
    /// </summary>
    public static bool PideResponsable(Rol rolDeLaInvitacion) => rolDeLaInvitacion == Rol.JUGADOR;

    /// <summary>
    /// Indica si el estado del club deja usar una invitación de ese rol (RF-021). Una invitación
    /// del club no sirve mientras su club está dado de baja; sí mientras está suspendido. Las de
    /// presidente no dependen del estado del club, como en la 001.
    /// </summary>
    public static bool ElClubPermiteUsarla(Rol rolDeLaInvitacion, EstadoClub estadoDelClub) =>
        !EsDelClub(rolDeLaInvitacion) || estadoDelClub != EstadoClub.DADO_DE_BAJA;
}
