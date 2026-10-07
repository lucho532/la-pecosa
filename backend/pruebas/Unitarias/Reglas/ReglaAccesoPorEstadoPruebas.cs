using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Pruebas.Unitarias.Reglas;

public class ReglaAccesoPorEstadoPruebas
{
    [Theory]
    [InlineData(Rol.PRESIDENTE)]
    [InlineData(Rol.DIRECTIVO)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.JUGADOR)]
    public void En_un_club_activo_entran_todos(Rol rol) =>
        Assert.Equal(ResultadoAcceso.Permitido, ReglaAccesoPorEstado.Evaluar(EstadoClub.ACTIVO, rol));

    [Fact]
    public void En_un_club_suspendido_entra_su_presidente() =>
        Assert.Equal(
            ResultadoAcceso.Permitido,
            ReglaAccesoPorEstado.Evaluar(EstadoClub.SUSPENDIDO, Rol.PRESIDENTE));

    [Theory]
    [InlineData(Rol.DIRECTIVO)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.JUGADOR)]
    public void En_un_club_suspendido_no_entra_nadie_mas(Rol rol) =>
        Assert.Equal(ResultadoAcceso.ClubSuspendido, ReglaAccesoPorEstado.Evaluar(EstadoClub.SUSPENDIDO, rol));

    [Theory]
    [InlineData(Rol.PRESIDENTE)]
    [InlineData(Rol.DIRECTIVO)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.JUGADOR)]
    public void En_un_club_dado_de_baja_no_entra_nadie(Rol rol) =>
        Assert.Equal(ResultadoAcceso.ClubDadoDeBaja, ReglaAccesoPorEstado.Evaluar(EstadoClub.DADO_DE_BAJA, rol));
}
