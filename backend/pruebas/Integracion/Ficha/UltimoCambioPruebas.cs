using System.Net;
using LaPecosa.Pruebas.Integracion.Base;
using LaPecosa.Pruebas.Integracion.Categorias;
using Microsoft.EntityFrameworkCore;

namespace LaPecosa.Pruebas.Integracion.Ficha;

/// <summary>
/// La ficha muestra quién la cambió por última vez y cuándo, y solo eso (historia 1, escenarios 9
/// y 10; RF-038; CE-012; supuestos 3 y 4 del plan).
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class UltimoCambioPruebas
{
    private readonly FabricaApi _fabrica;

    public UltimoCambioPruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Antes_del_primer_cambio_no_existe_la_propiedad_ni_la_fila()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);

        Assert.False((await e.FichaDeAsync(e.CuentaDeAna, e.Ana.Id)).Tiene("ultimoCambio"));
        Assert.False((await e.FichaDeAsync(e.Presidente, e.Ana.Id)).Tiene("ultimoCambio"));
        Assert.Null(await e.FichaGuardadaAsync(e.Ana.Id));
    }

    [Fact]
    public async Task Tras_guardar_la_familia_trae_la_fecha_su_nombre_y_que_fue_la_cuenta_del_jugador()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        var antes = DateTime.UtcNow.AddSeconds(-1);

        var respuesta = await e.CuentaDeAna.PutAsync(e.Ficha(e.Ana.Id), EscenarioFicha.Cuerpo());

        // Lo ve todo el que puede ver la ficha.
        foreach (var ficha in new[] { await respuesta.JsonAsync(), await e.FichaDeAsync(e.Directivo, e.Ana.Id), await e.FichaDeAsync(e.Entrenador, e.Ana.Id) })
        {
            var cambio = ficha.GetProperty("ultimoCambio");
            Assert.InRange(cambio.GetProperty("fecha").GetDateTime(), antes, DateTime.UtcNow.AddSeconds(1));
            Assert.Equal($"{e.Ana.Nombres} {e.Ana.Apellidos}", cambio.GetProperty("autor").GetString());
            Assert.True(cambio.GetProperty("porLaCuentaDelJugador").GetBoolean());
        }
    }

    // CE-012: el segundo cambio sustituye al primero; no hay historial.
    [Fact]
    public async Task Un_segundo_cambio_sustituye_al_primero()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        await _fabrica.Fichas.CrearFichaAsync(e.Ana, e.Base.IntegrantePresidente, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.False((await e.FichaDeAsync(e.CuentaDeAna, e.Ana.Id)).GetProperty("ultimoCambio").GetProperty("porLaCuentaDelJugador").GetBoolean());

        await e.CuentaDeAna.PutAsync(e.Ficha(e.Ana.Id), EscenarioFicha.Cuerpo());

        var cambio = (await e.FichaDeAsync(e.CuentaDeAna, e.Ana.Id)).GetProperty("ultimoCambio");
        Assert.True(cambio.GetProperty("fecha").GetDateTime() > new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(cambio.GetProperty("porLaCuentaDelJugador").GetBoolean());
        Assert.Equal(1, await FilasDeAsync(e.Ana.Id));
    }

    [Fact]
    public async Task Guardar_lo_mismo_dos_veces_no_mueve_la_fecha()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        await e.CuentaDeAna.PutAsync(e.Ficha(e.Ana.Id), EscenarioFicha.Cuerpo());
        var primera = (await e.FichaGuardadaAsync(e.Ana.Id))!.UltimoCambioEn;

        var respuesta = await e.Presidente.PutAsync(e.Ficha(e.Ana.Id), EscenarioFicha.Cuerpo());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var guardada = (await e.FichaGuardadaAsync(e.Ana.Id))!;
        Assert.Equal(primera, guardada.UltimoCambioEn);
        Assert.Equal(e.Ana.UsuarioId, guardada.UltimoCambioPorUsuarioId);
    }

    // Research §9: sin ningún dato distinto no se crea la fila.
    [Fact]
    public async Task Guardar_sin_cambiar_nada_en_una_ficha_vacia_no_crea_la_fila()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        var (cuenta, adulto) = await e.Base.Sembrar.CrearJugadorAsync(e.Club, 1995);
        var cliente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(cuenta);

        var respuesta = await cliente.PutAsync(e.Ficha(adulto.Id), new { celular = cuenta.Celular });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.False((await respuesta.JsonAsync()).Tiene("ultimoCambio"));
        Assert.Null(await e.FichaGuardadaAsync(adulto.Id));
    }

    [Fact]
    public async Task Cambiar_solo_el_celular_tambien_sella_el_ultimo_cambio()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        var (cuenta, adulto) = await e.Base.Sembrar.CrearJugadorAsync(e.Club, 1995);
        var cliente = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(cuenta);

        var respuesta = await cliente.PutAsync(e.Ficha(adulto.Id), new { celular = "3187776655" });

        Assert.True((await respuesta.JsonAsync()).Tiene("ultimoCambio"));
        Assert.Equal(cuenta.Id, (await e.FichaGuardadaAsync(adulto.Id))!.UltimoCambioPorUsuarioId);
    }

    private Task<int> FilasDeAsync(Guid usuarioRolId) => _fabrica.ConContextoAsync(contexto =>
        contexto.FichasJugador.IgnoreQueryFilters().CountAsync(ficha => ficha.UsuarioRolId == usuarioRolId));
}
