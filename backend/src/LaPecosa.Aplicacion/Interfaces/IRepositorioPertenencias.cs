using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa las consultas de la propia cuenta que cruzan clubes por necesidad: iniciar sesión y
/// elegir club (constitución §7.1, segunda excepción).
/// Su responsabilidad es responder, siempre acotado a una cuenta o a un documento, a qué clubes
/// pertenece una cuenta y de qué cuenta es un documento.
/// No lista integrantes de un club ni entrega datos de un club a quien no pertenece a él.
/// </summary>
public interface IRepositorioPertenencias
{
    /// <summary>Integrantes de una cuenta, cada uno con su club.</summary>
    Task<IReadOnlyList<UsuarioRol>> ListarDeUsuarioAsync(Guid usuarioId, CancellationToken cancelacion = default);

    /// <summary>Integrante de una cuenta en un club, con su club; nulo si no pertenece a él.</summary>
    Task<UsuarioRol?> ObtenerAsync(Guid usuarioId, Guid clubId, CancellationToken cancelacion = default);

    /// <summary>Cuenta dueña de un número de documento ya normalizado; nulo si nadie lo tiene.</summary>
    Task<Usuario?> ObtenerCuentaPorDocumentoAsync(string numeroDocumento, CancellationToken cancelacion = default);

    /// <summary>Indica si ese número de documento ya normalizado existe en el club.</summary>
    Task<bool> ExisteDocumentoEnClubAsync(Guid clubId, string numeroDocumento, CancellationToken cancelacion = default);

    /// <summary>
    /// Indica si ese número de documento ya normalizado es, en ese club, el de un jugador que el
    /// club retiró.
    /// </summary>
    Task<bool> EsDocumentoDeRetiradoAsync(Guid clubId, string numeroDocumento, CancellationToken cancelacion = default);

    /// <summary>
    /// Saca de inmediato a ese integrante de todos los equipos en los que juega, dentro de la
    /// transacción en curso. Se usa cuando deja de ser jugador al aceptar una invitación de
    /// presidente.
    /// </summary>
    Task SacarDeSusEquiposAsync(Guid usuarioRolId, CancellationToken cancelacion = default);

    /// <summary>Indica si la cuenta tiene algún integrante en algún club.</summary>
    Task<bool> TieneAlgunaAsync(Guid usuarioId, CancellationToken cancelacion = default);

    /// <summary>Añade un integrante nuevo; se guarda con la unidad de trabajo.</summary>
    void Agregar(UsuarioRol integrante);
}
