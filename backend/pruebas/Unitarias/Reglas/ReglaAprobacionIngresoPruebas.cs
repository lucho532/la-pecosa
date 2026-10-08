using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Pruebas.Unitarias.Reglas;

public class ReglaAprobacionIngresoPruebas
{
    private static readonly Guid QuienAprueba = Guid.NewGuid();
    private static readonly Guid Ingreso = Guid.NewGuid();

    // Toda la tabla quien-aprueba × rol elegido (RF-022, RF-023, CE-007).
    [Theory]
    [InlineData(Rol.PRESIDENTE, Rol.JUGADOR, true)]
    [InlineData(Rol.PRESIDENTE, Rol.ENTRENADOR, true)]
    [InlineData(Rol.PRESIDENTE, Rol.DIRECTIVO, true)]
    [InlineData(Rol.PRESIDENTE, Rol.PRESIDENTE, false)]
    [InlineData(Rol.PRESIDENTE, Rol.DESARROLLADOR, false)]
    [InlineData(Rol.DIRECTIVO, Rol.JUGADOR, true)]
    [InlineData(Rol.DIRECTIVO, Rol.ENTRENADOR, true)]
    [InlineData(Rol.DIRECTIVO, Rol.DIRECTIVO, false)]
    [InlineData(Rol.DIRECTIVO, Rol.PRESIDENTE, false)]
    [InlineData(Rol.DIRECTIVO, Rol.DESARROLLADOR, false)]
    [InlineData(Rol.ENTRENADOR, Rol.JUGADOR, false)]
    [InlineData(Rol.ENTRENADOR, Rol.ENTRENADOR, false)]
    [InlineData(Rol.ENTRENADOR, Rol.DIRECTIVO, false)]
    [InlineData(Rol.ENTRENADOR, Rol.PRESIDENTE, false)]
    [InlineData(Rol.JUGADOR, Rol.JUGADOR, false)]
    [InlineData(Rol.JUGADOR, Rol.ENTRENADOR, false)]
    [InlineData(Rol.JUGADOR, Rol.DIRECTIVO, false)]
    [InlineData(Rol.JUGADOR, Rol.PRESIDENTE, false)]
    [InlineData(Rol.DESARROLLADOR, Rol.JUGADOR, false)]
    [InlineData(Rol.DESARROLLADOR, Rol.DIRECTIVO, false)]
    public void Cada_quien_solo_asigna_los_roles_que_le_corresponden(Rol quienAprueba, Rol elegido, bool esperado) =>
        Assert.Equal(esperado, ReglaAprobacionIngreso.PuedeAsignar(quienAprueba, elegido));

    [Theory]
    [InlineData(Rol.PRESIDENTE, true)]
    [InlineData(Rol.DIRECTIVO, true)]
    [InlineData(Rol.ENTRENADOR, false)]
    [InlineData(Rol.JUGADOR, false)]
    [InlineData(Rol.DESARROLLADOR, false)]
    public void Solo_el_presidente_y_los_directivos_aprueban(Rol quienAprueba, bool esperado) =>
        Assert.Equal(esperado, ReglaAprobacionIngreso.PuedeAprobar(quienAprueba, QuienAprueba, Ingreso));

    [Theory]
    [InlineData(Rol.PRESIDENTE)]
    [InlineData(Rol.DIRECTIVO)]
    public void Nadie_aprueba_su_propio_ingreso(Rol quienAprueba) =>
        Assert.False(ReglaAprobacionIngreso.PuedeAprobar(quienAprueba, QuienAprueba, QuienAprueba));

    [Fact]
    public void El_rol_presidente_nunca_es_asignable()
    {
        foreach (var quienAprueba in Enum.GetValues<Rol>())
        {
            Assert.DoesNotContain(Rol.PRESIDENTE, ReglaAprobacionIngreso.RolesAsignablesPor(quienAprueba));
            Assert.DoesNotContain(Rol.DESARROLLADOR, ReglaAprobacionIngreso.RolesAsignablesPor(quienAprueba));
        }
    }
}
