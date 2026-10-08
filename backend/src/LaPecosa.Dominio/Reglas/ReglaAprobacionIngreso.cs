using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Dominio.Reglas;

/// <summary>
/// Representa la regla de quién aprueba un ingreso y qué rol puede dejar (constitución §8 y §12.2;
/// RF-021 a RF-023).
/// Su responsabilidad es responder qué roles puede asignar cada quien al aprobar: el PRESIDENTE,
/// JUGADOR, ENTRENADOR o DIRECTIVO; un DIRECTIVO, JUGADOR o ENTRENADOR; nadie más aprueba. El rol
/// PRESIDENTE nunca se asigna al aprobar, y nadie aprueba su propio ingreso.
/// No comprueba que el ingreso siga en espera ni que las dos personas sean del mismo club: eso lo
/// garantizan la autorización y la sentencia condicionada de la aprobación.
/// </summary>
public static class ReglaAprobacionIngreso
{
    private static readonly Rol[] DelPresidente = [Rol.JUGADOR, Rol.ENTRENADOR, Rol.DIRECTIVO];
    private static readonly Rol[] DelDirectivo = [Rol.JUGADOR, Rol.ENTRENADOR];

    /// <summary>Roles que puede dejar al aprobar quien tiene ese rol; vacío si no aprueba.</summary>
    public static IReadOnlyList<Rol> RolesAsignablesPor(Rol rolDeQuienAprueba) => rolDeQuienAprueba switch
    {
        Rol.PRESIDENTE => DelPresidente,
        Rol.DIRECTIVO => DelDirectivo,
        _ => [],
    };

    /// <summary>
    /// Indica si el integrante con ese rol puede aprobar ese ingreso: su rol aprueba y el ingreso
    /// no es el suyo.
    /// </summary>
    public static bool PuedeAprobar(Rol rolDeQuienAprueba, Guid integranteQueAprueba, Guid integranteDelIngreso) =>
        integranteQueAprueba != integranteDelIngreso && RolesAsignablesPor(rolDeQuienAprueba).Count > 0;

    /// <summary>Indica si quien tiene ese rol puede dejar a la persona con el rol elegido.</summary>
    public static bool PuedeAsignar(Rol rolDeQuienAprueba, Rol rolElegido) =>
        RolesAsignablesPor(rolDeQuienAprueba).Contains(rolElegido);
}
