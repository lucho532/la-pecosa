using System.Net;
using System.Text.Json;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Pruebas.Integracion.Base;

namespace LaPecosa.Pruebas.Integracion.Ingresos;

/// <summary>
/// Solo el PRESIDENTE o un DIRECTIVO aprueban un ingreso, y un DIRECTIVO solo asigna ENTRENADOR
/// (constitución §20, RF-021 a RF-026, CE-006 y CE-007).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class AprobacionPruebas
{
    private readonly FabricaApi _fabrica;

    public AprobacionPruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Theory]
    [InlineData(Rol.PRESIDENTE, Rol.JUGADOR)]
    [InlineData(Rol.PRESIDENTE, Rol.ENTRENADOR)]
    [InlineData(Rol.PRESIDENTE, Rol.DIRECTIVO)]
    [InlineData(Rol.DIRECTIVO, Rol.JUGADOR)]
    [InlineData(Rol.DIRECTIVO, Rol.ENTRENADOR)]
    public async Task Queda_aprobado_con_ese_unico_rol_y_con_quien_lo_aprobo_cuando_y_con_que_rol(Rol quienAprueba, Rol elegido)
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (aprobador, integranteAprobador) = await _fabrica.Sembrador.CrearIntegranteAsync(club, quienAprueba);
        var cliente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(aprobador);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var respuesta = await cliente.PostAsync(EscenarioIngresos.Aprobacion(club, enEspera.Id), new { rol = elegido.ToString() });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var nombreAprobador = $"{integranteAprobador.Nombres} {integranteAprobador.Apellidos}";
        var dto = await ClienteDePrueba.LeerAsync<JsonElement>(respuesta);
        Assert.Equal(enEspera.Id, dto.GetProperty("usuarioRolId").GetGuid());
        Assert.Equal(elegido.ToString(), dto.GetProperty("rolDeIngreso").GetString());
        Assert.Equal(nombreAprobador, dto.GetProperty("aprobadoPor").GetString());
        Assert.True(dto.GetProperty("aprobadoEn").GetDateTime() >= antes);

        var guardado = (await _fabrica.IntegranteAsync(enEspera.Id))!;
        Assert.Equal(EstadoIngreso.APROBADO, guardado.EstadoIngreso);
        Assert.Equal(elegido, guardado.Rol);
        Assert.Equal(elegido, guardado.RolDeIngreso);
        Assert.Equal(aprobador.Id, guardado.AprobadoPorUsuarioId);
        Assert.Equal(nombreAprobador, guardado.AprobadoPorNombre);
        Assert.NotNull(guardado.AprobadoEn);
        Assert.Empty(await cliente.ListaAsync(EscenarioIngresos.EnEspera(club)));
    }

    [Theory]
    [InlineData(Rol.DIRECTIVO, "DIRECTIVO")]
    [InlineData(Rol.DIRECTIVO, "PRESIDENTE")]
    [InlineData(Rol.PRESIDENTE, "PRESIDENTE")]
    [InlineData(Rol.PRESIDENTE, "DESARROLLADOR")]
    public async Task Un_rol_que_no_puede_asignar_responde_403_y_la_persona_sigue_en_espera(Rol quienAprueba, string elegido)
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var cliente = await _fabrica.ClienteDeAsync(club, quienAprueba);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);

        var respuesta = await cliente.PostAsync(EscenarioIngresos.Aprobacion(club, enEspera.Id), new { rol = elegido });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("rol_no_asignable", await ClienteDePrueba.CodigoAsync(respuesta));
        await ComprobarQueSigueEnEsperaAsync(enEspera.Id);
    }

    [Fact]
    public async Task Sin_rol_o_con_un_rol_que_no_existe_responde_400_y_la_persona_sigue_en_espera()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var cliente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var ruta = EscenarioIngresos.Aprobacion(club, enEspera.Id);

        foreach (var cuerpo in new object[] { new { }, new { rol = (string?)null }, new { rol = "UTILERO" }, new { rol = 4 } })
        {
            var respuesta = await cliente.PostAsync(ruta, cuerpo);

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Equal("datos_invalidos", await ClienteDePrueba.CodigoAsync(respuesta));
            Assert.True((await ClienteDePrueba.LeerAsync<JsonElement>(respuesta)).GetProperty("errores").TryGetProperty("rol", out _));
        }

        await ComprobarQueSigueEnEsperaAsync(enEspera.Id);
    }

    [Fact]
    public async Task Un_entrenador_un_jugador_otra_cuenta_en_espera_y_quien_no_tiene_sesion_no_aprueban()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var (otraEnEspera, suIngreso) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var delOtro = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(otraEnEspera);
        var ruta = EscenarioIngresos.Aprobacion(club, enEspera.Id);
        var cuerpo = new { rol = "JUGADOR" };

        foreach (var rol in new[] { Rol.ENTRENADOR, Rol.JUGADOR })
        {
            var respuesta = await (await _fabrica.ClienteDeAsync(club, rol)).PostAsync(ruta, cuerpo);
            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
            Assert.Equal("rol_no_autorizado", await ClienteDePrueba.CodigoAsync(respuesta));
        }

        // Quien está en espera no aprueba a otro ni se aprueba a sí mismo (RF-021).
        foreach (var rutaDelOtro in new[] { ruta, EscenarioIngresos.Aprobacion(club, suIngreso.Id) })
        {
            var respuesta = await delOtro.PostAsync(rutaDelOtro, cuerpo);
            Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
            Assert.Equal("ingreso_en_espera", await ClienteDePrueba.CodigoAsync(respuesta));
        }

        Assert.Equal(HttpStatusCode.Unauthorized, (await _fabrica.CrearClienteDePrueba().PostAsync(ruta, cuerpo)).StatusCode);
        await ComprobarQueSigueEnEsperaAsync(enEspera.Id);
        await ComprobarQueSigueEnEsperaAsync(suIngreso.Id);
    }

    [Fact]
    public async Task La_persona_aprobada_entra_con_el_mismo_token_de_sesion_que_tenia_en_espera()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var (cuenta, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        // El token se emite en espera y no se renueva en toda la prueba (RF-024).
        var suCliente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(cuenta);
        var token = suCliente.Token;
        Assert.Equal("ingreso_en_espera", await ClienteDePrueba.CodigoAsync(await suCliente.GetAsync($"/api/clubes/{club.Id}")));

        await presidente.PostAsync(EscenarioIngresos.Aprobacion(club, enEspera.Id), new { rol = "ENTRENADOR" });

        Assert.Equal(token, suCliente.Token);
        var respuesta = await suCliente.GetAsync($"/api/clubes/{club.Id}");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("ENTRENADOR", (await ClienteDePrueba.LeerAsync<JsonElement>(respuesta)).GetProperty("miRol").GetString());
        var sesion = await ClienteDePrueba.LeerAsync<JsonElement>(await suCliente.GetAsync("/api/sesion"));
        var suClub = Assert.Single(sesion.GetProperty("clubes").EnumerateArray());
        Assert.Equal("APROBADO", suClub.GetProperty("estadoIngreso").GetString());
        Assert.Equal("ENTRENADOR", suClub.GetProperty("rol").GetString());
    }

    [Fact]
    public async Task Aprobar_dos_veces_responde_409_y_no_cambia_quien_aprobo_ni_el_rol()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var directivo = await _fabrica.ClienteDeAsync(club, Rol.DIRECTIVO);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var ruta = EscenarioIngresos.Aprobacion(club, enEspera.Id);

        var primera = await presidente.PostAsync(ruta, new { rol = "DIRECTIVO" });
        var trasLaPrimera = (await _fabrica.IntegranteAsync(enEspera.Id))!;
        var segunda = await directivo.PostAsync(ruta, new { rol = "JUGADOR" });

        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
        Assert.Equal("ingreso_ya_aprobado", await ClienteDePrueba.CodigoAsync(segunda));
        var final = (await _fabrica.IntegranteAsync(enEspera.Id))!;
        Assert.Equal(Rol.DIRECTIVO, final.Rol);
        Assert.Equal(Rol.DIRECTIVO, final.RolDeIngreso);
        Assert.Equal(trasLaPrimera.AprobadoPorUsuarioId, final.AprobadoPorUsuarioId);
        Assert.Equal(trasLaPrimera.AprobadoPorNombre, final.AprobadoPorNombre);
        Assert.Equal(trasLaPrimera.AprobadoEn, final.AprobadoEn);
    }

    [Fact]
    public async Task Dos_aprobaciones_simultaneas_dan_un_200_y_un_409()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var directivo = await _fabrica.ClienteDeAsync(club, Rol.DIRECTIVO);
        var (_, enEspera) = await _fabrica.Sembrador.CrearIntegranteEnEsperaAsync(club);
        var ruta = EscenarioIngresos.Aprobacion(club, enEspera.Id);

        var respuestas = await Task.WhenAll(
            presidente.PostAsync(ruta, new { rol = "ENTRENADOR" }),
            directivo.PostAsync(ruta, new { rol = "JUGADOR" }));

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.Conflict],
            respuestas.Select(respuesta => respuesta.StatusCode).Order().ToList());
        var ganadora = respuestas.Single(respuesta => respuesta.StatusCode == HttpStatusCode.OK);
        var rolGanador = (await ClienteDePrueba.LeerAsync<JsonElement>(ganadora)).GetProperty("rolDeIngreso").GetString();
        Assert.Equal(rolGanador, (await _fabrica.IntegranteAsync(enEspera.Id))!.Rol.ToString());
    }

    [Fact]
    public async Task Un_integrante_inexistente_responde_404_y_uno_que_entro_sin_sala_de_espera_409()
    {
        var club = await _fabrica.Sembrador.CrearClubAsync();
        var presidente = await _fabrica.ClienteDeAsync(club, Rol.PRESIDENTE);
        var (_, yaAprobado) = await _fabrica.Sembrador.CrearIntegranteAsync(club, Rol.JUGADOR);

        var inexistente = await presidente.PostAsync(EscenarioIngresos.Aprobacion(club, Guid.NewGuid()), new { rol = "JUGADOR" });
        var aprobado = await presidente.PostAsync(EscenarioIngresos.Aprobacion(club, yaAprobado.Id), new { rol = "DIRECTIVO" });

        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        Assert.Equal("no_encontrado", await ClienteDePrueba.CodigoAsync(inexistente));
        Assert.Equal(HttpStatusCode.Conflict, aprobado.StatusCode);
        Assert.Equal("ingreso_ya_aprobado", await ClienteDePrueba.CodigoAsync(aprobado));
        Assert.Equal(Rol.JUGADOR, (await _fabrica.IntegranteAsync(yaAprobado.Id))!.Rol);
    }

    private async Task ComprobarQueSigueEnEsperaAsync(Guid usuarioRolId)
    {
        var integrante = (await _fabrica.IntegranteAsync(usuarioRolId))!;
        Assert.Equal(EstadoIngreso.EN_ESPERA, integrante.EstadoIngreso);
        Assert.Equal(Rol.JUGADOR, integrante.Rol);
        Assert.Null(integrante.AprobadoEn);
        Assert.Null(integrante.AprobadoPorUsuarioId);
        Assert.Null(integrante.AprobadoPorNombre);
        Assert.Null(integrante.RolDeIngreso);
    }
}
