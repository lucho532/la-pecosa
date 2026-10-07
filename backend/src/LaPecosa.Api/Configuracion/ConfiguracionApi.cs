using System.Text.Json.Serialization;
using LaPecosa.Api.Errores;
using Microsoft.OpenApi;

namespace LaPecosa.Api.Configuracion;

/// <summary>
/// Representa la configuración de los controladores y de Swagger.
/// Su responsabilidad es fijar el formato JSON del contrato (enumeraciones como texto), responder
/// <c>datos_invalidos</c> cuando el cuerpo no se puede leer y documentar la API con el esquema de
/// seguridad <c>sesion</c>.
/// No registra servicios de negocio.
/// </summary>
public static class ConfiguracionApi
{
    /// <summary>Registra controladores, manejo de errores y Swagger.</summary>
    public static IServiceCollection AgregarApi(this IServiceCollection servicios)
    {
        servicios
            .AddControllers(opciones =>
            {
                // La obligatoriedad la deciden los validadores de Aplicacion, con mensajes en español.
                opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .AddJsonOptions(opciones =>
            {
                opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            })
            .ConfigureApiBehaviorOptions(opciones =>
            {
                opciones.InvalidModelStateResponseFactory = ManejadorErrores.DatosInvalidos;
            });

        servicios.AddExceptionHandler<ManejadorErrores>();
        servicios.AddProblemDetails();

        servicios.AddEndpointsApiExplorer();
        servicios.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo { Title = "La Pecosa", Version = "0.1.0" });
            opciones.AddSecurityDefinition("sesion", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });
            opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("sesion", documento)] = [],
            });
        });

        return servicios;
    }
}
