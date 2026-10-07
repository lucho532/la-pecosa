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
        return servicios;
    }
}
