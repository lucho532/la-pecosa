namespace LaPecosa.Dominio.Enumeraciones;

/// <summary>
/// Representa el estado de ingreso de un integrante en su club (constitución §12.1.1).
/// Su responsabilidad es distinguir a quien espera aprobación de quien ya fue aprobado.
/// No describe el estado del club ni el de la cuenta.
/// </summary>
public enum EstadoIngreso
{
    /// <summary>El ingreso está pendiente de aprobación.</summary>
    EN_ESPERA,

    /// <summary>El ingreso fue aprobado y el integrante entra a su club.</summary>
    APROBADO,
}
