using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Entidades;

/// <summary>
/// Representa la pertenencia de una persona a un club: lo que la especificación llama
/// "integrante" (constitución §12.3).
/// Su responsabilidad es guardar el único rol de la persona en ese club, su identidad en él
/// (nombre, documento y fecha de nacimiento), su estado de ingreso y, cuando alguien del club lo
/// aprobó, quién lo hizo, cuándo y con qué rol.
/// No guarda el correo ni la contraseña, que son de la cuenta, y nunca lleva el rol DESARROLLADOR.
/// No guarda un historial de estados ni ningún rastro de un rechazo: quien es rechazado se borra.
/// </summary>
public class UsuarioRol : IPerteneceAClub
{
    /// <summary>Clave primaria.</summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <inheritdoc />
    public Guid ClubId { get; set; }

    /// <summary>Club al que pertenece.</summary>
    public Club? Club { get; set; }

    /// <summary>Cuenta de la persona.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Cuenta de la persona.</summary>
    public Usuario? Usuario { get; set; }

    /// <summary>Único rol en el club: PRESIDENTE, DIRECTIVO, ENTRENADOR o JUGADOR.</summary>
    public Rol Rol { get; set; }

    /// <summary>
    /// Estado de ingreso. Ya existía, siempre <c>APROBADO</c>. Ahora puede ser <c>EN_ESPERA</c>
    /// (RF-014): quien entra con una invitación del club espera a que lo aprueben.
    /// </summary>
    public EstadoIngreso EstadoIngreso { get; set; }

    /// <summary>
    /// Fecha y hora de la aprobación, en UTC. Nulo mientras está en espera y en quien entró sin
    /// sala de espera. Los cuatro datos de la aprobación se rellenan juntos y no cambian después.
    /// </summary>
    public DateTime? AprobadoEn { get; set; }

    /// <summary>
    /// Cuenta de quien aprobó. Opcional. Foránea a <see cref="Usuario"/>; pasa a nulo si esa
    /// cuenta se elimina.
    /// </summary>
    public Guid? AprobadoPorUsuarioId { get; set; }

    /// <summary>Nombres y apellidos de quien aprobó, copiados en ese momento (§13).</summary>
    public string? AprobadoPorNombre { get; set; }

    /// <summary>
    /// Rol con el que quedó al aprobarse: <c>JUGADOR</c>, <c>ENTRENADOR</c> o <c>DIRECTIVO</c>
    /// (RF-025). No cambia aunque después cambie <see cref="Rol"/>.
    /// </summary>
    public Rol? RolDeIngreso { get; set; }

    /// <summary>Nombres. Obligatorio.</summary>
    public string Nombres { get; set; } = string.Empty;

    /// <summary>Apellidos. Obligatorio.</summary>
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>Tipo de documento (§10).</summary>
    public TipoDocumento TipoDocumento { get; set; }

    /// <summary>
    /// Número de documento, sin espacios ni puntos y en minúsculas. Único dentro del club (RF-017).
    /// </summary>
    public string NumeroDocumento { get; set; } = string.Empty;

    /// <summary>Fecha de nacimiento. Obligatoria y no futura.</summary>
    public DateOnly FechaNacimiento { get; set; }

    /// <summary>Fecha de creación, en UTC.</summary>
    public DateTime CreadoEn { get; set; }
}
