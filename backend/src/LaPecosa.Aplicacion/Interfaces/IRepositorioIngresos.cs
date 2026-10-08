using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Interfaces;

/// <summary>
/// Representa el acceso a los ingresos de un club: su sala de espera y sus ingresos aprobados.
/// Su responsabilidad es leer los integrantes en espera y los aprobados del club de la petición,
/// que fijó la autorización en <see cref="IContextoClub"/>, y aprobar o borrar un ingreso con una
/// sola sentencia condicionada a que siga en espera, para que entre dos acciones simultáneas valga
/// la primera (RF-026).
/// No recibe un identificador de club ni ve integrantes de otro club. No decide quién puede aprobar
/// ni qué rol puede asignar: eso lo hacen la autorización y las reglas del dominio.
/// </summary>
public interface IRepositorioIngresos
{
    /// <summary>
    /// La sala de espera: los integrantes en espera del club, cada uno con su cuenta, del registro
    /// más antiguo al más reciente.
    /// </summary>
    Task<IReadOnlyList<UsuarioRol>> ListarEnEsperaAsync(CancellationToken cancelacion = default);

    /// <summary>
    /// Los ingresos aprobados: los integrantes a los que alguien del club aprobó, del más reciente
    /// al más antiguo. No incluye a quien entró sin sala de espera.
    /// </summary>
    Task<IReadOnlyList<UsuarioRol>> ListarAprobadosAsync(CancellationToken cancelacion = default);

    /// <summary>Busca un integrante del club por su identificador, con su cuenta; no lo sigue.</summary>
    Task<UsuarioRol?> ObtenerAsync(Guid usuarioRolId, CancellationToken cancelacion = default);

    /// <summary>
    /// Aprueba el ingreso solo si sigue en espera: en una única sentencia lo deja aprobado, con su
    /// rol definitivo y con quién lo aprobó, cuándo y con qué rol. Devuelve si lo aprobó.
    /// </summary>
    Task<bool> AprobarAsync(
        Guid usuarioRolId,
        Rol rol,
        DateTime ahoraUtc,
        Guid aprobadoPorUsuarioId,
        string aprobadoPorNombre,
        CancellationToken cancelacion = default);

    /// <summary>
    /// Borra de inmediato al integrante solo si sigue en espera, dentro de la transacción en
    /// curso. Devuelve si lo borró.
    /// </summary>
    Task<bool> BorrarSiSigueEnEsperaAsync(Guid usuarioRolId, CancellationToken cancelacion = default);
}
