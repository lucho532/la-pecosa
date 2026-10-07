using LaPecosa.Aplicacion.Implementaciones;
using LaPecosa.Aplicacion.Servicios;

namespace LaPecosa.Api.Configuracion;

/// <summary>
/// Representa el registro de los casos de uso de Aplicacion.
/// Su responsabilidad es conectar cada <c>IServicio</c> con su implementación.
/// No registra infraestructura ni contiene lógica.
/// </summary>
public static class RegistroDeCasosDeUso
{
    /// <summary>Registra los servicios de los casos de uso.</summary>
    public static IServiceCollection AgregarCasosDeUso(this IServiceCollection servicios)
    {
        servicios.AddScoped<IServicioSesion, ServicioSesion>();
        servicios.AddScoped<IServicioRecuperacion, ServicioRecuperacion>();

        servicios.AddScoped<IServicioConsultaClubes, ServicioConsultaClubes>();
        servicios.AddScoped<IServicioCreacionClub, ServicioCreacionClub>();
        servicios.AddScoped<IServicioInvitacionPresidente, ServicioInvitacionPresidente>();
        servicios.AddScoped<IServicioRetiroPresidente, ServicioRetiroPresidente>();

        servicios.AddScoped<IServicioConsultaClub, ServicioConsultaClub>();
        servicios.AddScoped<IServicioRegistroConInvitacion, ServicioRegistroConInvitacion>();
        servicios.AddScoped<IServicioAceptacionInvitacion, ServicioAceptacionInvitacion>();

        return servicios;
    }
}
