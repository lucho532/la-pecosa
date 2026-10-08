using System.Net;
using System.Text.Json;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Pruebas.Integracion.Base;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Pruebas.Integracion.Ingresos;

/// <summary>
/// Quien se registra con una invitación del club queda como JUGADOR y en espera (constitución §20,
/// RF-009 a RF-014), y una invitación no sirve dos veces, vencida ni cancelada.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class RegistroEnEsperaPruebas
{
    private const string Contrasena = "mi-contrasena-propia";

    private readonly FabricaApi _fabrica;

    public RegistroEnEsperaPruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Con_una_invitacion_del_club_queda_jugador_en_espera_y_entra_con_correo_y_con_documento()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (invitacion, token) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var documento = Sembrador.Unico("doc");
        var cliente = _fabrica.CrearClienteDePrueba();

        var respuesta = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, documento, responsable: "  Marta   Gómez "));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var integrante = await IntegranteAsync(club.Id, documento);
        Assert.Equal(Rol.JUGADOR, integrante.Rol);
        Assert.Equal(EstadoIngreso.EN_ESPERA, integrante.EstadoIngreso);
        Assert.Null(integrante.AprobadoEn);
        Assert.Null(integrante.RolDeIngreso);
        var cuenta = await CuentaAsync(integrante.UsuarioId);
        Assert.Equal(invitacion.Correo, cuenta.CorreoNormalizado);
        Assert.Equal("Marta Gómez", cuenta.NombreResponsable);

        foreach (var identificador in new[] { invitacion.Correo, documento })
        {
            var otro = _fabrica.CrearClienteDePrueba();
            Assert.Equal(HttpStatusCode.OK, (await otro.IniciarSesionAsync(identificador, Contrasena)).StatusCode);
            var sesion = await ClienteDePrueba.LeerAsync<JsonElement>(await otro.GetAsync("/api/sesion"));
            var unico = Assert.Single(sesion.GetProperty("clubes").EnumerateArray());
            Assert.Equal("EN_ESPERA", unico.GetProperty("estadoIngreso").GetString());
            Assert.Equal("ingreso_en_espera", await ClienteDePrueba.CodigoAsync(await otro.GetAsync($"/api/clubes/{club.Id}")));
        }
    }

    [Fact]
    public async Task Sin_token_no_hay_registro_y_una_invitacion_usada_vencida_cancelada_o_reemplazada_responde_410()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (presidente, _) = await _fabrica.Sembrador.CrearIntegranteAsync(club, Rol.PRESIDENTE);
        var delPresidente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(presidente);
        var (_, usada) = await _fabrica.Sembrador.CrearInvitacionAsync(club, usadaEn: DateTime.UtcNow, rol: Rol.JUGADOR);
        var (_, vencida) = await _fabrica.Sembrador.CrearInvitacionAsync(club, venceEn: DateTime.UtcNow.AddSeconds(-1), rol: Rol.JUGADOR);
        var (porCancelar, cancelada) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var (porReemplazar, reemplazada) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        await delPresidente.PostAsync($"/api/clubes/{club.Id}/invitaciones/{porCancelar.Id}/cancelacion");
        await delPresidente.PostAsync($"/api/clubes/{club.Id}/invitaciones", new { correo = porReemplazar.Correo });
        var cliente = _fabrica.CrearClienteDePrueba();
        var antes = await ContarIntegrantesAsync(club.Id);

        foreach (var token in new[] { usada, vencida, cancelada, reemplazada, "no-existe", "" })
        {
            var consulta = await cliente.PostAsync("/api/invitaciones/consulta", new { token });
            var registro = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, Sembrador.Unico("doc")));

            Assert.Equal(HttpStatusCode.Gone, consulta.StatusCode);
            Assert.Equal(HttpStatusCode.Gone, registro.StatusCode);
            Assert.Equal("invitacion_no_valida", await ClienteDePrueba.CodigoAsync(registro));
        }

        Assert.Equal(antes, await ContarIntegrantesAsync(club.Id));
    }

    [Fact]
    public async Task Un_correo_un_rol_o_un_club_enviados_en_el_cuerpo_se_ignoran()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var otroClub = await _fabrica.Sembrador.CrearClubAsync();
        var (invitacion, token) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var documento = Sembrador.Unico("doc");
        var cuerpo = JsonSerializer.Deserialize<JsonElement>(
            $"{{\"token\":\"{token}\",\"correo\":\"intruso@lapecosa.test\",\"rol\":\"PRESIDENTE\",\"estadoIngreso\":\"APROBADO\"," +
            $"\"clubId\":\"{otroClub.Id}\",\"nombres\":\"Ana\",\"apellidos\":\"Pérez\",\"tipoDocumento\":\"CEDULA_CIUDADANIA\"," +
            $"\"numeroDocumento\":\"{documento}\",\"fechaNacimiento\":\"1990-01-01\",\"celular\":\"3001234567\",\"contrasena\":\"{Contrasena}\"}}");

        var respuesta = await _fabrica.CrearClienteDePrueba().PostAsync("/api/invitaciones/registro", cuerpo);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var integrante = await IntegranteAsync(club.Id, documento);
        Assert.Equal(Rol.JUGADOR, integrante.Rol);
        Assert.Equal(EstadoIngreso.EN_ESPERA, integrante.EstadoIngreso);
        Assert.Equal(invitacion.Correo, (await CuentaAsync(integrante.UsuarioId)).CorreoNormalizado);
        Assert.Equal(0, await ContarIntegrantesAsync(otroClub.Id));
        Assert.False(await _fabrica.ConContextoAsync(contexto => contexto.Usuarios.AnyAsync(u => u.CorreoNormalizado == "intruso@lapecosa.test")));
    }

    [Fact]
    public async Task Un_menor_sin_responsable_responde_400_y_no_gasta_la_invitacion_y_con_responsable_se_registra()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (_, token) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var cliente = _fabrica.CrearClienteDePrueba();
        var haceDiezAnos = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)).ToString("yyyy-MM-dd");
        var documento = Sembrador.Unico("doc");

        var sinResponsable = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, documento, haceDiezAnos));
        var demasiadoLargo = await cliente.PostAsync(
            "/api/invitaciones/registro", Datos(token, documento, haceDiezAnos, new string('a', 161)));
        var conResponsable = await cliente.PostAsync(
            "/api/invitaciones/registro", Datos(token, documento, haceDiezAnos, "Marta Gómez"));

        foreach (var invalida in new[] { sinResponsable, demasiadoLargo })
        {
            Assert.Equal(HttpStatusCode.BadRequest, invalida.StatusCode);
            Assert.Equal("datos_invalidos", await ClienteDePrueba.CodigoAsync(invalida));
            Assert.True((await ClienteDePrueba.LeerAsync<JsonElement>(invalida)).GetProperty("errores").TryGetProperty("nombreResponsable", out _));
        }

        Assert.Equal(HttpStatusCode.Created, conResponsable.StatusCode);
        Assert.Equal(EstadoIngreso.EN_ESPERA, (await IntegranteAsync(club.Id, documento)).EstadoIngreso);
    }

    [Fact]
    public async Task El_documento_repetido_en_el_club_o_de_otra_cuenta_responde_409()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var otroClub = await _fabrica.Sembrador.CrearClubAsync();
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var (_, deOtroClub) = await _fabrica.Sembrador.CrearIntegranteAsync(otroClub, Rol.JUGADOR);
        var (_, token) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var cliente = _fabrica.CrearClienteDePrueba();

        var repetido = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, enEspera.NumeroDocumento));
        var deOtraCuenta = await cliente.PostAsync("/api/invitaciones/registro", Datos(token, deOtroClub.NumeroDocumento));

        Assert.Equal(HttpStatusCode.Conflict, repetido.StatusCode);
        Assert.Equal("documento_repetido_en_club", await ClienteDePrueba.CodigoAsync(repetido));
        Assert.Equal(HttpStatusCode.Conflict, deOtraCuenta.StatusCode);
        Assert.Equal("documento_en_otra_cuenta", await ClienteDePrueba.CodigoAsync(deOtraCuenta));
    }

    [Fact]
    public async Task La_consulta_dice_si_pasa_por_la_sala_de_espera_y_la_de_presidente_sigue_entrando_aprobada()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (_, delClub) = await _fabrica.Sembrador.CrearInvitacionAsync(club, rol: Rol.JUGADOR);
        var (_, dePresidente) = await _fabrica.Sembrador.CrearInvitacionAsync(club);
        var cliente = _fabrica.CrearClienteDePrueba();
        var documento = Sembrador.Unico("doc");

        var consultaDelClub = await ClienteDePrueba.LeerAsync<JsonElement>(
            await cliente.PostAsync("/api/invitaciones/consulta", new { token = delClub }));
        var consultaDePresidente = await ClienteDePrueba.LeerAsync<JsonElement>(
            await cliente.PostAsync("/api/invitaciones/consulta", new { token = dePresidente }));
        var registro = await cliente.PostAsync("/api/invitaciones/registro", Datos(dePresidente, documento));

        Assert.True(consultaDelClub.GetProperty("pasaPorSalaDeEspera").GetBoolean());
        Assert.False(consultaDePresidente.GetProperty("pasaPorSalaDeEspera").GetBoolean());
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);
        var presidente = await IntegranteAsync(club.Id, documento);
        Assert.Equal(Rol.PRESIDENTE, presidente.Rol);
        Assert.Equal(EstadoIngreso.APROBADO, presidente.EstadoIngreso);
        Assert.Null(presidente.AprobadoEn);
    }

    private static object Datos(
        string token, string numeroDocumento, string fechaNacimiento = "1988-03-15", string? responsable = null) => new
        {
            token,
            nombres = "Ana",
            apellidos = "Pérez",
            tipoDocumento = "CEDULA_CIUDADANIA",
            numeroDocumento,
            fechaNacimiento,
            celular = "3001234567",
            nombreResponsable = responsable,
            contrasena = Contrasena,
        };

    private Task<UsuarioRol> IntegranteAsync(Guid clubId, string documento) => _fabrica.ConContextoAsync(contexto =>
        contexto.UsuariosRol.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(i => i.ClubId == clubId && i.NumeroDocumento == documento));

    private Task<Usuario> CuentaAsync(Guid usuarioId) => _fabrica.ConContextoAsync(contexto =>
        contexto.Usuarios.AsNoTracking().SingleAsync(usuario => usuario.Id == usuarioId));

    private Task<int> ContarIntegrantesAsync(Guid clubId) => _fabrica.ConContextoAsync(contexto =>
        contexto.UsuariosRol.IgnoreQueryFilters().CountAsync(i => i.ClubId == clubId));
}
