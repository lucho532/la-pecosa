using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Dominio.Entidades;

namespace LaPecosa.Aplicacion.Servicios;

/// <summary>
/// Representa el caso de uso de aprobar un ingreso en espera (constitución §12.1.1 y §12.2; RF-021
/// a RF-026).
/// Su responsabilidad es dejar a la persona aprobada con el rol elegido, que pasa a ser su único
/// rol en el club, y registrar quién la aprobó, cuándo y con qué rol, una sola vez.
/// A quien queda como JUGADOR lo ubica en la categoría activa de su año de nacimiento, si el club
/// la tiene (RF-008 y RF-009 de la 003).
/// No asigna equipo, no genera cobros y no envía correos. No cambia el rol de quien ya está
/// aprobado: no existe ninguna operación para eso.
/// </summary>
public interface IServicioAprobacionIngreso
{
    /// <summary>
    /// Aprueba el ingreso con el rol indicado. <paramref name="quienAprueba"/> es el integrante de
    /// la sesión en ese club, ya comprobado por la autorización.
    /// </summary>
    Task<IngresoAprobadoDto> AprobarAsync(
        Guid usuarioRolId, AprobarIngresoDto datos, UsuarioRol quienAprueba, CancellationToken cancelacion = default);
}
