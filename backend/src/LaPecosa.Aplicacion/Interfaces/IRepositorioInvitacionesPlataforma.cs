using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el acceso a las invitaciones desde el panel de administración de la plataforma
/// (constitución §7.1, primera excepción).
/// Su responsabilidad es guardar, listar y anular las invitaciones de un club.
/// No busca invitaciones por su token (eso es del registro) ni decide si una está vigente.
/// </summary>
public interface IRepositorioInvitacionesPlataforma
{
    /// <summary>Invitaciones de un club sin usar ni anular, de la más reciente a la más antigua.</summary>
    Task<IReadOnlyList<Invitacion>> ListarPendientesAsync(Guid clubId, CancellationToken cancelacion = default);

    /// <summary>Busca una invitación de un club por su identificador.</summary>
    Task<Invitacion?> ObtenerAsync(Guid clubId, Guid invitacionId, CancellationToken cancelacion = default);

    /// <summary>Anula las invitaciones sin usar ni anular de ese club para ese correo normalizado.</summary>
    Task AnularPendientesAsync(
        Guid clubId, string correoNormalizado, DateTime ahoraUtc, CancellationToken cancelacion = default);

    /// <summary>Añade una invitación nueva; se guarda con la unidad de trabajo.</summary>
    void Agregar(Invitacion invitacion);
}
