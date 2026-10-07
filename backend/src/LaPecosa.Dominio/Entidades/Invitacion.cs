using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Entidades;

/// <summary>
/// Representa el enlace enviado por correo para registrarse en un club con un rol (constitución
/// §12.1 y §12.5).
/// Su responsabilidad es ligar un club, un rol y un correo a un token de un solo uso, del que solo
/// se guarda el hash, y saber si sigue vigente.
/// No guarda el token en claro ni crea la cuenta: eso lo hace el registro.
/// </summary>
public class Invitacion : IPerteneceAClub
{
    /// <summary>Días de vigencia de una invitación (RF-012).</summary>
    public const int DiasDeVigencia = 7;

    /// <summary>Clave primaria.</summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <inheritdoc />
    public Guid ClubId { get; set; }

    /// <summary>Club al que invita.</summary>
    public Club? Club { get; set; }

    /// <summary>Rol con el que entra quien la usa. En esta funcionalidad, siempre PRESIDENTE (RF-012).</summary>
    public Rol Rol { get; set; }

    /// <summary>Correo invitado, guardado normalizado. El registro usa siempre este correo.</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>SHA-256 del token. Único. El token en claro no se guarda.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Resultado del envío del correo.</summary>
    public EstadoEnvio EstadoEnvio { get; set; } = EstadoEnvio.PENDIENTE;

    /// <summary>Cuenta que la envió.</summary>
    public Guid CreadaPorUsuarioId { get; set; }

    /// <summary>Fecha de creación, en UTC.</summary>
    public DateTime CreadaEn { get; set; }

    /// <summary>Fecha de vencimiento: la de creación más 7 días.</summary>
    public DateTime VenceEn { get; set; }

    /// <summary>Fecha de uso. Nulo hasta que se usa.</summary>
    public DateTime? UsadaEn { get; set; }

    /// <summary>Fecha de anulación. Nulo hasta que otra la reemplaza.</summary>
    public DateTime? AnuladaEn { get; set; }

    /// <summary>Vigente: sin usar, sin anular y antes de su vencimiento. No se guarda.</summary>
    public bool EstaVigente(DateTime ahoraUtc) => UsadaEn is null && AnuladaEn is null && ahoraUtc < VenceEn;

    /// <summary>Vencida: ni usada ni anulada, pero ya pasó su vencimiento. No se guarda.</summary>
    public bool EstaVencida(DateTime ahoraUtc) => UsadaEn is null && AnuladaEn is null && ahoraUtc >= VenceEn;
}
