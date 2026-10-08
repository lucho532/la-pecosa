using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Infraestructura.Correo;

namespace LaPecosa.Pruebas.Unitarias.Correo;

public class PlantillasCorreoPruebas
{
    private const string UrlBase = "http://localhost:5173";
    private const string Club = "Valfor F.C.";
    private const string Token = "token-de-prueba";

    [Fact]
    public void La_invitacion_de_presidente_nombra_su_rol()
    {
        var mensaje = PlantillasCorreo.Invitacion(UrlBase, Club, Rol.PRESIDENTE, Token);

        Assert.Contains("presidente", mensaje.Texto);
        Assert.Contains(Club, mensaje.Texto);
        Assert.Equal($"{UrlBase}/invitacion#{Token}", mensaje.Enlace);
        Assert.Contains(mensaje.Enlace, mensaje.Texto);
    }

    [Theory]
    [InlineData(Rol.JUGADOR)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.DIRECTIVO)]
    public void La_invitacion_del_club_lleva_el_club_y_el_enlace_y_no_nombra_ningun_rol(Rol rol)
    {
        var mensaje = PlantillasCorreo.Invitacion($"{UrlBase}/", Club, rol, Token);

        Assert.Contains(Club, mensaje.Texto);
        Assert.Contains(Club, mensaje.Asunto);
        Assert.Equal($"{UrlBase}/invitacion#{Token}", mensaje.Enlace);
        Assert.Contains(mensaje.Enlace, mensaje.Texto);

        foreach (var nombreDeRol in new[] { "jugador", "presidente", "entrenador", "directivo", "desarrollador" })
        {
            Assert.DoesNotContain(nombreDeRol, mensaje.Texto, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(nombreDeRol, mensaje.Asunto, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Las_dos_invitaciones_avisan_de_que_el_enlace_sirve_una_vez_y_vence()
    {
        foreach (var rol in new[] { Rol.PRESIDENTE, Rol.JUGADOR })
        {
            var mensaje = PlantillasCorreo.Invitacion(UrlBase, Club, rol, Token);

            Assert.Contains("una sola vez", mensaje.Texto);
            Assert.Contains("7 días", mensaje.Texto);
        }
    }
}
