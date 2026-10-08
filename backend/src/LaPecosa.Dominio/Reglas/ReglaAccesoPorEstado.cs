using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa la regla que decide quién entra a un club según el estado del club y el estado de
/// ingreso de la persona (constitución §7.4 y §12.1.1; RF-027 y RF-028 de la 001, RF-016 de la 002).
/// Su responsabilidad es responder si un integrante puede entrar. Mira primero el estado del club:
/// en uno activo entran todos; en uno suspendido, solo su PRESIDENTE; en uno dado de baja, nadie.
/// Solo después mira el estado de ingreso: quien está en espera no entra. Por ese orden, una
/// cuenta en espera de un club suspendido o dado de baja recibe el motivo del club.
/// No comprueba que la persona pertenezca al club ni conoce HTTP.
/// </summary>
public static class ReglaAccesoPorEstado
{
    /// <summary>
    /// Evalúa el acceso de un integrante con ese rol y ese estado de ingreso a un club en ese
    /// estado.
    /// </summary>
    public static ResultadoAcceso Evaluar(EstadoClub estado, Rol rol, EstadoIngreso estadoIngreso)
    {
        var porEstadoDelClub = estado switch
        {
            EstadoClub.ACTIVO => ResultadoAcceso.Permitido,
            EstadoClub.SUSPENDIDO when rol == Rol.PRESIDENTE => ResultadoAcceso.Permitido,
            EstadoClub.SUSPENDIDO => ResultadoAcceso.ClubSuspendido,
            _ => ResultadoAcceso.ClubDadoDeBaja,
        };

        return porEstadoDelClub == ResultadoAcceso.Permitido && estadoIngreso == EstadoIngreso.EN_ESPERA
            ? ResultadoAcceso.IngresoEnEspera
            : porEstadoDelClub;
    }
}
