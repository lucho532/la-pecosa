using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Pruebas.Unitarias.Reglas;

public class ReglaIngresoPorInvitacionPruebas
{
    [Fact]
    public void La_invitacion_de_presidente_entra_aprobada_y_sin_sala_de_espera()
    {
        Assert.Equal(EstadoIngreso.APROBADO, ReglaIngresoPorInvitacion.EstadoDeIngreso(Rol.PRESIDENTE));
        Assert.False(ReglaIngresoPorInvitacion.PasaPorSalaDeEspera(Rol.PRESIDENTE));
    }

    [Theory]
    [InlineData(Rol.JUGADOR)]
    [InlineData(Rol.ENTRENADOR)]
    [InlineData(Rol.DIRECTIVO)]
    public void Cualquier_otra_invitacion_deja_en_espera(Rol rol)
    {
        Assert.Equal(EstadoIngreso.EN_ESPERA, ReglaIngresoPorInvitacion.EstadoDeIngreso(rol));
        Assert.True(ReglaIngresoPorInvitacion.PasaPorSalaDeEspera(rol));
    }

    [Theory]
    [InlineData(EstadoClub.ACTIVO, true)]
    [InlineData(EstadoClub.SUSPENDIDO, true)]
    [InlineData(EstadoClub.DADO_DE_BAJA, false)]
    public void La_invitacion_del_club_no_sirve_mientras_el_club_esta_dado_de_baja(EstadoClub estado, bool esperado) =>
        Assert.Equal(esperado, ReglaIngresoPorInvitacion.ElClubPermiteUsarla(Rol.JUGADOR, estado));

    [Theory]
    [InlineData(EstadoClub.ACTIVO)]
    [InlineData(EstadoClub.SUSPENDIDO)]
    [InlineData(EstadoClub.DADO_DE_BAJA)]
    public void La_invitacion_de_presidente_no_depende_del_estado_del_club(EstadoClub estado) =>
        Assert.True(ReglaIngresoPorInvitacion.ElClubPermiteUsarla(Rol.PRESIDENTE, estado));
}
