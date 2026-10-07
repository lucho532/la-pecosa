using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using Microsoft.Extensions.DependencyInjection;

namespace LaPecosa.Pruebas.Integracion.Base;

/// <summary>
/// Crea clubes, cuentas e integrantes directamente en la base de datos. Todos los nombres, correos
/// y documentos que genera son únicos, porque las pruebas comparten la base de datos.
/// </summary>
public class Sembrador
{
    public const string Contrasena = "Contrasena-de-prueba-1";

    private static int _contador;
    private readonly FabricaApi _fabrica;

    public Sembrador(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    /// <summary>Texto único para nombres, correos y documentos.</summary>
    public static string Unico(string prefijo = "") =>
        $"{prefijo}{Interlocked.Increment(ref _contador)}{Guid.NewGuid():N}"[..(prefijo.Length + 12)];

    public static string CorreoUnico() => $"{Unico("persona")}@lapecosa.test";

    public async Task<Club> CrearClubAsync(string? nombre = null, EstadoClub estado = EstadoClub.ACTIVO)
    {
        nombre ??= $"Club {Unico()}";
        var club = new Club
        {
            Nombre = nombre,
            NombreNormalizado = NormalizadorTexto.NombreClub(nombre),
            Estado = estado,
            CreadoEn = DateTime.UtcNow,
        };

        return await _fabrica.ConContextoAsync(async contexto =>
        {
            contexto.Clubes.Add(club);
            await contexto.SaveChangesAsync();
            return club;
        });
    }

    public async Task<Usuario> CrearCuentaAsync(string? correo = null, string? contrasena = Contrasena)
    {
        correo ??= CorreoUnico();
        var hash = _fabrica.Services.GetRequiredService<IHashContrasena>();
        var usuario = new Usuario
        {
            Correo = correo,
            CorreoNormalizado = NormalizadorTexto.Correo(correo),
            ContrasenaHash = contrasena is null ? null : hash.Calcular(contrasena),
            Celular = "3001234567",
            CreadoEn = DateTime.UtcNow,
        };

        return await _fabrica.ConContextoAsync(async contexto =>
        {
            contexto.Usuarios.Add(usuario);
            await contexto.SaveChangesAsync();
            return usuario;
        });
    }

    /// <summary>Añade a una cuenta que ya existe como integrante de un club.</summary>
    public async Task<UsuarioRol> CrearIntegranteAsync(
        Club club, Usuario usuario, Rol rol, string? documento = null, string nombres = "Ana")
    {
        var integrante = new UsuarioRol
        {
            ClubId = club.Id,
            UsuarioId = usuario.Id,
            Rol = rol,
            EstadoIngreso = EstadoIngreso.APROBADO,
            Nombres = nombres,
            Apellidos = "Pérez",
            TipoDocumento = TipoDocumento.CEDULA_CIUDADANIA,
            NumeroDocumento = NormalizadorTexto.Documento(documento ?? Unico("doc")),
            FechaNacimiento = new DateOnly(1990, 5, 20),
            CreadoEn = DateTime.UtcNow,
        };

        return await _fabrica.ConContextoAsync(async contexto =>
        {
            contexto.UsuariosRol.Add(integrante);
            await contexto.SaveChangesAsync();
            return integrante;
        });
    }

    /// <summary>Crea una cuenta nueva y la añade como integrante de un club.</summary>
    public async Task<(Usuario Usuario, UsuarioRol Integrante)> CrearIntegranteAsync(Club club, Rol rol)
    {
        var usuario = await CrearCuentaAsync();
        return (usuario, await CrearIntegranteAsync(club, usuario, rol));
    }
}
