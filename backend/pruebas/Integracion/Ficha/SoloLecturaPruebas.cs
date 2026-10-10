using System.Net;
using LaPecosa.Dominio.Enumeraciones;
using LaPecosa.Pruebas.Integracion.Base;

namespace LaPecosa.Pruebas.Integracion.Ficha;

/// <summary>
/// Un DIRECTIVO y un ENTRENADOR no cambian ningún dato de ninguna ficha, tampoco el entrenador de
/// la categoría del jugador (historia 2, escenario 7; RF-019; CE-005). Reciben
/// <c>403 rol_no_autorizado</c> antes de que se mire el identificador.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class SoloLecturaPruebas
{
    private readonly FabricaApi _fabrica;

    public SoloLecturaPruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task No_guardan_el_formulario_de_la_ficha()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);

        foreach (var (rol, cliente) in e.QuienesSoloConsultan)
        {
            // Ana está en la categoría que entrena el entrenador; Beto, no.
            foreach (var jugador in new[] { e.Ana, e.Beto })
            {
                var respuesta = await cliente.PutAsync(e.Ficha(jugador.Id), EscenarioFicha.Cuerpo());

                await NegadoAsync(respuesta, rol);
                Assert.Null(await e.FichaGuardadaAsync(jugador.Id));
                Assert.Equal("3001234567", (await e.JugadorGuardadoAsync(jugador.Id)).Usuario!.Celular);
            }
        }
    }

    [Fact]
    public async Task No_suben_ni_reemplazan_documentos()
    {
        var e = await EscenarioFicha.CrearAsync(_fabrica);
        var anterior = SembradorFichas.Pdf(64);
        await _fabrica.Fichas.CrearDocumentoAsync(e.Ana, DocumentoPedido.CERTIFICADO_SALUD, anterior, "application/pdf");

        foreach (var (rol, cliente) in e.QuienesSoloConsultan)
        {
            foreach (var documento in Enum.GetValues<DocumentoPedido>())
            {
                await NegadoAsync(await cliente.SubirAsync(e.Documento(e.Ana.Id, documento), SembradorFichas.Png()), rol);
            }
        }

        Assert.Equal(anterior, Assert.Single(await e.DocumentosGuardadosAsync(e.Ana.Id)).Contenido);
        Assert.Null(await e.FichaGuardadaAsync(e.Ana.Id));
    }

    private static async Task NegadoAsync(HttpResponseMessage respuesta, string rol)
    {
        Assert.True(respuesta.StatusCode == HttpStatusCode.Forbidden, $"{rol}: {(int)respuesta.StatusCode}");
        Assert.Equal("rol_no_autorizado", await ClienteDePrueba.CodigoAsync(respuesta));
    }
}
