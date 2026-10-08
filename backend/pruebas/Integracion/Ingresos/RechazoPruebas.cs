using System.Net;
using System.Text.Json;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Pruebas.Integracion.Base;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Pruebas.Integracion.Ingresos;

/// <summary>
/// Rechazar un ingreso borra a la persona de ese club, no afecta a sus otros clubes y le permite
/// volver solo con una invitación nueva (constitución §20, RF-027, RF-027a y RF-027b).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class RechazoPruebas
{
    private const string Contrasena = "mi-contrasena-propia";

    private readonly FabricaApi _fabrica;

    public RechazoPruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Theory]
    [InlineData(Rol.PRESIDENTE)]
    [InlineData(Rol.DIRECTIVO)]
    public async Task Si_era_su_unico_club_no_queda_nada_suyo_y_solo_vuelve_con_una_invitacion_nueva(Rol quienRechaza)
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var cliente = await _fabrica.ClienteDeAsync(club, quienRechaza);
        var correo = Sembrador.CorreoUnico();
        var documento = Sembrador.Unico("doc");
        var (suCliente, tokenUsado) = await InvitarYRegistrarAsync(cliente, club, correo, documento);
        var enEspera = Assert.Single(await cliente.ListaAsync(EscenarioIngresos.EnEspera(club)));
        var usuarioRolId = enEspera.GetProperty("usuarioRolId").GetGuid();
        var usuarioId = (await _fabrica.IntegranteAsync(usuarioRolId))!.UsuarioId;
        var correosAntes = _fabrica.Correo.Enviados.Count;

        var respuesta = await cliente.PostAsync(EscenarioIngresos.Rechazo(club, usuarioRolId));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Empty(await cliente.ListaAsync(EscenarioIngresos.EnEspera(club)));
        Assert.Null(await _fabrica.IntegranteAsync(usuarioRolId));
        Assert.Empty(await cliente.ListaAsync($"/api/clubes/{club.Id}/invitaciones"));
        Assert.False(await _fabrica.ExisteCuentaAsync(usuarioId));
        // No se le avisa: ni correo, ni mensaje distinto al de cualquier inicio de sesión fallido.
        Assert.Equal(correosAntes, _fabrica.Correo.Enviados.Count);
        Assert.Equal(HttpStatusCode.Unauthorized, (await suCliente.GetAsync("/api/sesion")).StatusCode);
        var entrar = await _fabrica.CrearClienteDePrueba().IniciarSesionAsync(correo, Contrasena);
        Assert.Equal("credenciales_invalidas", await ClienteDePrueba.CodigoAsync(entrar));

        // Sin invitación nueva no puede volver; con ella, sí, con el mismo correo y documento (RF-027b).
        var conLaAnterior = await _fabrica.CrearClienteDePrueba().PostAsync("/api/invitaciones/registro", Datos(tokenUsado, documento));
        Assert.Equal(HttpStatusCode.Gone, conLaAnterior.StatusCode);
        await InvitarYRegistrarAsync(cliente, club, correo, documento);
        var deNuevo = Assert.Single(await cliente.ListaAsync(EscenarioIngresos.EnEspera(club)));
        Assert.Equal(documento, deNuevo.GetProperty("numeroDocumento").GetString());
        Assert.NotEqual(usuarioRolId, deNuevo.GetProperty("usuarioRolId").GetGuid());
    }

    [Fact]
    public async Task Si_tiene_otro_club_lo_conserva_intacto_y_deja_de_ver_el_que_la_rechazo()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var otroClub = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var cuenta = await _fabrica.Sembrador.CrearCuentaAsync();
        var documento = Sembrador.Unico("doc");
        var enOtro = await _fabrica.Sembrador.CrearIntegranteAsync(otroClub, cuenta, Rol.ENTRENADOR, documento);
        var enEspera = await _fabrica.Sembrador.CrearIntegranteAsync(
            club, cuenta, Rol.JUGADOR, documento, estadoIngreso: EstadoIngreso.EN_ESPERA);
        var (delClub, _) = await _fabrica.Sembrador.CrearInvitacionAsync(club, cuenta.Correo, usadaEn: DateTime.UtcNow, rol: Rol.JUGADOR);
        var (dePresidente, _) = await _fabrica.Sembrador.CrearInvitacionAsync(club, cuenta.Correo);
        var (delOtroClub, _) = await _fabrica.Sembrador.CrearInvitacionAsync(otroClub, cuenta.Correo, usadaEn: DateTime.UtcNow, rol: Rol.JUGADOR);
        var suCliente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(cuenta);

        var respuesta = await presidente.PostAsync(EscenarioIngresos.Rechazo(club, enEspera.Id));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.Null(await _fabrica.IntegranteAsync(enEspera.Id));
        Assert.True(await _fabrica.ExisteCuentaAsync(cuenta.Id));
        var conservado = (await _fabrica.IntegranteAsync(enOtro.Id))!;
        Assert.Equal(Rol.ENTRENADOR, conservado.Rol);
        Assert.Equal(EstadoIngreso.APROBADO, conservado.EstadoIngreso);

        // Con la misma sesión: solo ve el otro club, y el que la rechazó le responde 404.
        var sesion = await ClienteDePrueba.LeerAsync<JsonElement>(await suCliente.GetAsync("/api/sesion"));
        var unico = Assert.Single(sesion.GetProperty("clubes").EnumerateArray());
        Assert.Equal(otroClub.Id, unico.GetProperty("clubId").GetGuid());
        Assert.Equal(HttpStatusCode.OK, (await suCliente.GetAsync($"/api/clubes/{otroClub.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await suCliente.GetAsync($"/api/clubes/{club.Id}")).StatusCode);

        // Solo se borran las invitaciones de este club a su correo; no la de presidente ni las de otro club.
        Assert.False(await ExisteInvitacionAsync(delClub.Id));
        Assert.True(await ExisteInvitacionAsync(dePresidente.Id));
        Assert.True(await ExisteInvitacionAsync(delOtroClub.Id));
    }

    [Fact]
    public async Task Rechazar_a_un_integrante_aprobado_responde_409_y_no_borra_nada()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var (cuenta, aprobado) = await _fabrica.Sembrador.CrearIntegranteAsync(club, Rol.JUGADOR);
        var (_, reciente) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        await presidente.PostAsync(EscenarioIngresos.Aprobacion(club, reciente.Id), new { rol = "DIRECTIVO" });

        foreach (var usuarioRolId in new[] { aprobado.Id, reciente.Id })
        {
            var respuesta = await presidente.PostAsync(EscenarioIngresos.Rechazo(club, usuarioRolId));

            Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
            Assert.Equal("ingreso_ya_aprobado", await ClienteDePrueba.CodigoAsync(respuesta));
            Assert.NotNull(await _fabrica.IntegranteAsync(usuarioRolId));
        }

        Assert.True(await _fabrica.ExisteCuentaAsync(cuenta.Id));
    }

    [Fact]
    public async Task Un_entrenador_un_jugador_una_cuenta_en_espera_y_quien_no_tiene_sesion_no_rechazan()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var (otra, _) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var ruta = EscenarioIngresos.Rechazo(club, enEspera.Id);
        var casos = new (ClienteDePrueba Cliente, HttpStatusCode Estado, string Codigo)[]
        {
            (await _fabrica.ClienteDeAsync(club, Rol.ENTRENADOR), HttpStatusCode.Forbidden, "rol_no_autorizado"),
            (await _fabrica.ClienteDeAsync(club, Rol.JUGADOR), HttpStatusCode.Forbidden, "rol_no_autorizado"),
            (await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(otra), HttpStatusCode.Forbidden, "ingreso_en_espera"),
            (_fabrica.CrearClienteDePrueba(), HttpStatusCode.Unauthorized, "sin_sesion"),
        };

        foreach (var (cliente, estado, codigo) in casos)
        {
            var respuesta = await cliente.PostAsync(ruta);

            Assert.Equal(estado, respuesta.StatusCode);
            Assert.Equal(codigo, await ClienteDePrueba.CodigoAsync(respuesta));
        }

        Assert.Equal(EstadoIngreso.EN_ESPERA, (await _fabrica.IntegranteAsync(enEspera.Id))!.EstadoIngreso);
    }

    [Fact]
    public async Task Rechazar_dos_veces_o_a_alguien_que_no_existe_responde_404()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);

        var primera = await presidente.PostAsync(EscenarioIngresos.Rechazo(club, enEspera.Id));
        var segunda = await presidente.PostAsync(EscenarioIngresos.Rechazo(club, enEspera.Id));
        var inexistente = await presidente.PostAsync(EscenarioIngresos.Rechazo(club, Guid.NewGuid()));
        var aprobarDespues = await presidente.PostAsync(EscenarioIngresos.Aprobacion(club, enEspera.Id), new { rol = "JUGADOR" });

        Assert.Equal(HttpStatusCode.NoContent, primera.StatusCode);
        foreach (var respuesta in new[] { segunda, inexistente, aprobarDespues })
        {
            Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
            Assert.Equal("no_encontrado", await ClienteDePrueba.CodigoAsync(respuesta));
        }
    }

    [Fact]
    public async Task Aprobar_y_rechazar_a_la_vez_a_la_misma_persona_vale_la_primera_accion()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var directivo = await _fabrica.ClienteDeAsync(club, Rol.DIRECTIVO);
        var (cuenta, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);

        var respuestas = await Task.WhenAll(
            presidente.PostAsync(EscenarioIngresos.Aprobacion(club, enEspera.Id), new { rol = "JUGADOR" }),
            directivo.PostAsync(EscenarioIngresos.Rechazo(club, enEspera.Id)));
        var (aprobacion, rechazo) = (respuestas[0].StatusCode, respuestas[1].StatusCode);

        var final = await _fabrica.IntegranteAsync(enEspera.Id);
        if (aprobacion == HttpStatusCode.OK)
        {
            Assert.Equal(HttpStatusCode.Conflict, rechazo);
            Assert.Equal(EstadoIngreso.APROBADO, final!.EstadoIngreso);
            Assert.True(await _fabrica.ExisteCuentaAsync(cuenta.Id));
        }
        else
        {
            Assert.Equal(HttpStatusCode.NoContent, rechazo);
            Assert.Equal(HttpStatusCode.NotFound, aprobacion);
            Assert.Null(final);
            Assert.False(await _fabrica.ExisteCuentaAsync(cuenta.Id));
        }
    }

    private async Task<(ClienteDePrueba Cliente, string Token)> InvitarYRegistrarAsync(
        ClienteDePrueba deQuienInvita, Club club, string correo, string documento)
    {
        var invitar = await deQuienInvita.PostAsync($"/api/clubes/{club.Id}/invitaciones", new { correo });
        Assert.Equal(HttpStatusCode.Created, invitar.StatusCode);
        var token = _fabrica.Correo.UltimoToken("invitacion", correo);
        var cliente = _fabrica.CrearClienteDePrueba();
        var registro = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, documento));
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        cliente.UsarToken((await ClienteDePrueba.LeerAsync<JsonElement>(registro)).GetProperty("token").GetString()!);
        return (cliente, token);
    }

    private static object Datos(string token, string numeroDocumento) => new
    {
        token,
        nombres = "Ana",
        apellidos = "Pérez",
        tipoDocumento = "CEDULA_CIUDADANIA",
        numeroDocumento,
        fechaNacimiento = "1988-03-15",
        celular = "3001234567",
        contrasena = Contrasena,
    };

    private Task<bool> ExisteInvitacionAsync(Guid invitacionId) => _fabrica.ConContextoAsync(contexto =>
        contexto.Invitaciones.IgnoreQueryFilters().AnyAsync(invitacion => invitacion.Id == invitacionId));
}
