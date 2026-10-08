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
        Assert.Equal(ResultadoAcceso.Permitido, Aprobado(EstadoClub.ACTIVO, rol));

    [Fact]
    public void En_un_club_suspendido_entra_su_presidente() =>
        Assert.Equal(ResultadoAcceso.Permitido, Aprobado(EstadoClub.SUSPENDIDO, Rol.PRESIDENTE));

    [Theory]
    [InlineData(Rol.DIRECTIVO)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.JUGADOR)]
    public void En_un_club_suspendido_no_entra_nadie_mas(Rol rol) =>
        Assert.Equal(ResultadoAcceso.ClubSuspendido, Aprobado(EstadoClub.SUSPENDIDO, rol));

    [Theory]
    [InlineData(Rol.PRESIDENTE)]
    [InlineData(Rol.DIRECTIVO)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.JUGADOR)]
    public void En_un_club_dado_de_baja_no_entra_nadie(Rol rol) =>
        Assert.Equal(ResultadoAcceso.ClubDadoDeBaja, Aprobado(EstadoClub.DADO_DE_BAJA, rol));

    [Fact]
    public void En_un_club_activo_quien_esta_en_espera_no_entra() =>
        Assert.Equal(ResultadoAcceso.IngresoEnEspera, EnEspera(EstadoClub.ACTIVO));

    // Primero el estado del club y después el de ingreso (supuesto 2 de research.md).
    [Fact]
    public void En_un_club_suspendido_quien_esta_en_espera_recibe_el_motivo_del_club() =>
        Assert.Equal(ResultadoAcceso.ClubSuspendido, EnEspera(EstadoClub.SUSPENDIDO));

    [Fact]
    public void En_un_club_dado_de_baja_quien_esta_en_espera_recibe_el_motivo_del_club() =>
        Assert.Equal(ResultadoAcceso.ClubDadoDeBaja, EnEspera(EstadoClub.DADO_DE_BAJA));

    private static ResultadoAcceso Aprobado(EstadoClub estado, Rol rol) =>
        ReglaAccesoPorEstado.Evaluar(estado, rol, EstadoIngreso.APROBADO);

    private static ResultadoAcceso EnEspera(EstadoClub estado) =>
        ReglaAccesoPorEstado.Evaluar(estado, Rol.JUGADOR, EstadoIngreso.EN_ESPERA);
}
