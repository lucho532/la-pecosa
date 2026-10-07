using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa la regla que decide quién entra a un club según su estado (constitución §7.4,
/// RF-027 y RF-028).
/// Su responsabilidad es responder si un integrante con un rol puede entrar: en un club activo
/// entran todos; en uno suspendido, solo su PRESIDENTE; en uno dado de baja, nadie.
/// No comprueba que la persona pertenezca al club ni conoce HTTP.
/// </summary>
public static class ReglaAccesoPorEstado
{
    /// <summary>Evalúa el acceso de un integrante con ese rol a un club en ese estado.</summary>
    public static ResultadoAcceso Evaluar(EstadoClub estado, Rol rol) => estado switch
    {
        EstadoClub.ACTIVO => ResultadoAcceso.Permitido,
        EstadoClub.SUSPENDIDO when rol == Rol.PRESIDENTE => ResultadoAcceso.Permitido,
        EstadoClub.SUSPENDIDO => ResultadoAcceso.ClubSuspendido,
        _ => ResultadoAcceso.ClubDadoDeBaja,
    };
}
