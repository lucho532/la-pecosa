namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa los errores del contrato que comparten el registro con invitación y la aceptación de
/// una invitación.
/// Su responsabilidad es que ambos casos de uso respondan con el mismo código y el mismo texto.
/// No decide cuándo se produce cada error.
/// </summary>
public static class ErroresDeInvitacion
{
    /// <summary>410: la invitación ya se usó, venció, fue reemplazada o no existe.</summary>
    public static ExcepcionDeAplicacion NoValida() => new(
        "invitacion_no_valida",
        410,
        "Esta invitación ya no sirve: ya se usó, venció o fue reemplazada por otra. Pide al club que te envíe una nueva.");

    /// <summary>409: el documento ya lo tiene otro integrante de ese club.</summary>
    public static ExcepcionDeAplicacion DocumentoRepetidoEnClub() => ExcepcionDeAplicacion.Conflicto(
        "documento_repetido_en_club", "Ya hay alguien registrado en este club con ese número de documento.");

    /// <summary>
    /// Traduce una violación de un índice único al error del contrato que corresponde, o devuelve
    /// nulo si el índice no es de los que el registro puede provocar.
    /// </summary>
    public static ExcepcionDeAplicacion? DeIndiceUnico(string? nombreIndice) => nombreIndice switch
    {
        IndicesUnicos.DocumentoEnClub => DocumentoRepetidoEnClub(),
        IndicesUnicos.CorreoDeUsuario => CorreoYaRegistrado(),
        _ => null,
    };

    /// <summary>409: el correo de la invitación ya tiene cuenta.</summary>
    public static ExcepcionDeAplicacion CorreoYaRegistrado() => ExcepcionDeAplicacion.Conflicto(
        "correo_ya_registrado",
        "Ya hay una cuenta registrada con ese correo. Inicia sesión con ella para aceptar la invitación.");
}
