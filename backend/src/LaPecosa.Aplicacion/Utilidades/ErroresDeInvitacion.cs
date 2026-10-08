namespace LaPecosa.Aplicacion.Utilidades;

/// <summary>
/// Representa los errores del contrato que comparten los casos de uso de las invitaciones:
/// enviarlas (desde el panel o desde el club), registrarse con una y aceptarla.
/// Su responsabilidad es que todos respondan con el mismo código y el mismo texto.
/// No decide cuándo se produce cada error.
/// </summary>
public static class ErroresDeInvitacion
{
    /// <summary>
    /// 410: la invitación ya se usó, venció, fue cancelada o reemplazada, no existe o su club está
    /// dado de baja. El texto no distingue el motivo.
    /// </summary>
    public static ExcepcionDeAplicacion NoValida() => new(
        "invitacion_no_valida",
        410,
        "Esta invitación ya no sirve: ya se usó, venció, fue cancelada o reemplazada por otra, o el club no está " +
        "disponible. Pide al club que te envíe una nueva.");

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

    /// <summary>
    /// 409: la cuenta ya es integrante del club de una invitación del club. No se cambia nada: la
    /// invitación del club no reemplaza el rol de nadie.
    /// </summary>
    public static ExcepcionDeAplicacion YaPertenecesAlClub() => ExcepcionDeAplicacion.Conflicto(
        "ya_perteneces_al_club", "Ya perteneces a este club. No hace falta que aceptes esta invitación.");

    /// <summary>409: el correo es el de la cuenta DESARROLLADOR, que no pertenece a ningún club.</summary>
    public static ExcepcionDeAplicacion CorreoDelDesarrollador() => ExcepcionDeAplicacion.Conflicto(
        "correo_del_desarrollador",
        "Ese correo es el de la administración de la plataforma y no puede pertenecer a ningún club.");

    /// <summary>409: el correo de la invitación ya tiene cuenta.</summary>
    public static ExcepcionDeAplicacion CorreoYaRegistrado() => ExcepcionDeAplicacion.Conflicto(
        "correo_ya_registrado",
        "Ya hay una cuenta registrada con ese correo. Inicia sesión con ella para aceptar la invitación.");
}
