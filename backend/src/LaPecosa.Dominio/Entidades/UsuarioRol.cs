using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Entidades;

/// <summary>
/// Representa la pertenencia de una persona a un club: lo que la especificación llama
/// "integrante" (constitución §12.3).
/// Su responsabilidad es guardar el único rol de la persona en ese club, su identidad en él
/// (nombre, documento y fecha de nacimiento), su estado de ingreso y, cuando alguien del club lo
/// aprobó, quién lo hizo, cuándo y con qué rol. De un jugador guarda además su categoría actual y,
/// si el club lo retiró, quién lo hizo y cuándo (constitución §11.2 y §14.1): todavía no existe la
/// entidad Jugador y aquí el jugador es el integrante aprobado con el rol JUGADOR.
/// No guarda el correo ni la contraseña, que son de la cuenta, y nunca lleva el rol DESARROLLADOR.
/// No guarda un historial de estados, de categorías ni de retiros, ni ningún rastro de un rechazo:
/// quien es rechazado se borra.
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

    /// <summary>
    /// Opcional. Categoría actual del jugador (RF-013). Solo puede tener valor en un jugador del
    /// club (<see cref="EsJugadorDelClub"/>), y la categoría debe ser de su mismo club y estar
    /// activa (RF-007, RF-012).
    /// </summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>Categoría actual del jugador.</summary>
    public Categoria? Categoria { get; set; }

    /// <summary>
    /// Obligatorio. Verdadero por defecto, también para las filas que ya existen. Falso significa
    /// retirado del club (§14.1); solo puede ser falso en un JUGADOR aprobado (RF-041).
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Opcional. Fecha y hora del retiro, en UTC. Solo tiene valor mientras está retirado. Los
    /// tres datos del retiro se rellenan juntos al retirar y se vacían juntos al reincorporar.
    /// </summary>
    public DateTime? RetiradoEn { get; set; }

    /// <summary>
    /// Opcional. Cuenta de quien lo retiró. Foránea a <see cref="Usuario"/>; pasa a nulo si esa
    /// cuenta se elimina.
    /// </summary>
    public Guid? RetiradoPorUsuarioId { get; set; }

    /// <summary>
    /// Opcional. Nombres y apellidos de quien lo retiró, copiados en ese momento (§13).
    /// </summary>
    public string? RetiradoPorNombre { get; set; }

    /// <summary>Equipos de su categoría en los que juega; puede no haber ninguno (RF-026).</summary>
    public List<JugadorEquipo> Equipos { get; set; } = [];

    /// <summary>
    /// Indica si es un jugador del club: rol JUGADOR, ingreso aprobado y no retirado. Solo ellos
    /// tienen categoría y equipos (RF-012).
    /// </summary>
    public bool EsJugadorDelClub =>
        Rol == Rol.JUGADOR && EstadoIngreso == EstadoIngreso.APROBADO && Activo;
}
