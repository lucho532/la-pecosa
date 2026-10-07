using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Infraestructura.Correo;

/// <summary>
/// Representa los textos en español de los correos de la plataforma.
/// Su responsabilidad es redactar la invitación a un club y la recuperación de contraseña, con su
/// enlace. El token va en el fragmento del enlace para que no quede en los registros del servidor
/// web (research §8).
/// No envía correos ni guarda tokens.
/// </summary>
public static class PlantillasCorreo
{
    /// <summary>Redacta la invitación para registrarse en un club con un rol.</summary>
    public static MensajeCorreo Invitacion(string urlBaseFrontend, string nombreClub, Rol rol, string token)
    {
        var enlace = $"{urlBaseFrontend.TrimEnd('/')}/invitacion#{token}";
        var texto =
            $"Hola:\n\n" +
            $"Te invitaron a La Pecosa como {NombreDeRol(rol)} de {nombreClub}.\n\n" +
            $"Para aceptar la invitación, abre este enlace:\n{enlace}\n\n" +
            "El enlace sirve una sola vez y vence en 7 días. Si no esperabas este correo, ignóralo.";

        return new MensajeCorreo($"Invitación a {nombreClub} en La Pecosa", texto, enlace);
    }

    /// <summary>Redacta el correo de recuperación de contraseña.</summary>
    public static MensajeCorreo Recuperacion(string urlBaseFrontend, string token)
    {
        var enlace = $"{urlBaseFrontend.TrimEnd('/')}/restablecer#{token}";
        var texto =
            $"Hola:\n\n" +
            $"Para crear una contraseña nueva en La Pecosa, abre este enlace:\n{enlace}\n\n" +
            "El enlace sirve una sola vez y vence en 60 minutos. Si no lo pediste, ignora este correo: " +
            "tu contraseña no cambia.";

        return new MensajeCorreo("Crea tu contraseña de La Pecosa", texto, enlace);
    }

    private static string NombreDeRol(Rol rol) => rol switch
    {
        Rol.PRESIDENTE => "presidente",
        Rol.DIRECTIVO => "directivo",
        Rol.ENTRENADOR => "entrenador",
        Rol.JUGADOR => "jugador",
        _ => rol.ToString().ToLowerInvariant(),
    };
}
