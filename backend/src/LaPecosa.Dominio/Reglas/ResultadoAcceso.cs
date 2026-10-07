namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa el resultado de evaluar el acceso de un integrante a su club.
/// Su responsabilidad es distinguir el acceso permitido de los dos motivos de rechazo.
/// No contiene el mensaje ni el código de error que ve la persona.
/// </summary>
public enum ResultadoAcceso
{
    /// <summary>El integrante puede entrar.</summary>
    Permitido,

    /// <summary>El club está suspendido y el integrante no es su presidente.</summary>
    ClubSuspendido,

    /// <summary>El club está dado de baja: no entra nadie.</summary>
    ClubDadoDeBaja,
}
