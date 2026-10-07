using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Entidades;

/// <summary>
/// Representa la pertenencia de una persona a un club: lo que la especificación llama
/// "integrante" (constitución §12.3).
/// Su responsabilidad es guardar el único rol de la persona en ese club y su identidad en él
/// (nombre, documento y fecha de nacimiento).
/// No guarda el correo ni la contraseña, que son de la cuenta, y nunca lleva el rol DESARROLLADOR.
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

    /// <summary>Estado de ingreso. En esta funcionalidad, siempre aprobado (RF-016).</summary>
    public EstadoIngreso EstadoIngreso { get; set; }

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
