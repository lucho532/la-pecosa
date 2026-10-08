using System.Net;
using System.Text;
using LaPecosa.Pruebas.Integracion.Base;

namespace LaPecosa.Pruebas.Integracion.Categorias;

/// <summary>
/// Solamente el PRESIDENTE crea, desactiva, reactiva y borra categorías y equipos, asigna
/// entrenadores, cambia a un jugador de categoría o de equipo y retira o reincorpora jugadores
/// (constitución §20, RF-036, CE-006). Recorre todos los endpoints de escritura que la propia API
/// expone, con identificadores reales, así que uno nuevo queda cubierto sin tocar esta prueba.
/// </summary>
[Collection(ColeccionApi.Nombre)]
public class SoloPresidentePruebas
{
    private readonly FabricaApi _fabrica;

    public SoloPresidentePruebas(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    /// <summary>Endpoints de categorías y jugadores que cambian algo.</summary>
    private List<Endpoint> DeEscritura => EndpointsDeLaApi.DeLaApi(_fabrica)
        .Where(endpoint => endpoint.Metodo != "GET"
            && (endpoint.Ruta.StartsWith("/api/clubes/{clubId}/categorias", StringComparison.Ordinal)
                || endpoint.Ruta.StartsWith("/api/clubes/{clubId}/jugadores", StringComparison.Ordinal)))
        .ToList();

    [Fact]
    public async Task Nadie_que_no_sea_el_presidente_cambia_nada_en_ningun_endpoint_de_escritura()
    {
        var e = await EscenarioCategorias.CrearAsync(_fabrica);
        var categoria = await e.Sembrar.CrearCategoriaAsync(e.Club, EscenarioCategorias.AnioDelJugador);
        var equipo = await e.Sembrar.CrearEquipoAsync(categoria, "A");
        var (usuarioJugador, jugador) = await e.Sembrar.CrearJugadorAsync(e.Club, 2014, categoria);
        await e.Sembrar.PonerEnEquipoAsync(jugador, equipo);
        var asignacion = await e.Sembrar.AsignarEntrenadorAsync(categoria, e.IntegranteEntrenador);
        await e.Sembrar.DirigirEquipoAsync(asignacion, equipo);
        var (enEspera, _) = await e.Sembrar.CrearJugadorEnEsperaAsync(e.Club, 2014);
        var delJugador = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(usuarioJugador);
        var delQueEspera = await _fabrica.CrearClienteDePrueba().ConSesionDeAsync(enEspera);
        Assert.Equal(16, DeEscritura.Count);

        // Los más cercanos a tener permiso: quien consulta todo, quien entrena esa categoría y dirige
        // ese equipo, y un jugador de esa categoría y de ese equipo.
        var quienes = new (ClienteDePrueba Cliente, string Codigo)[]
        {
            (e.Directivo, "rol_no_autorizado"), (e.Entrenador, "rol_no_autorizado"),
            (delJugador, "rol_no_autorizado"), (delQueEspera, "ingreso_en_espera"),
        };

        foreach (var endpoint in DeEscritura)
        {
            var ruta = endpoint.Ruta
                .Replace("{clubId}", e.Club.Id.ToString())
                .Replace("{categoriaId}", categoria.Id.ToString())
                .Replace("{equipoId}", equipo.Id.ToString())
                .Replace("{usuarioRolId}", jugador.Id.ToString());

            foreach (var (cliente, codigo) in quienes)
            {
                var respuesta = await EnviarAsync(cliente, endpoint.Metodo, ruta);
                Assert.True(respuesta.StatusCode == HttpStatusCode.Forbidden, $"{endpoint}: {(int)respuesta.StatusCode}");
                Assert.Equal(codigo, await ClienteDePrueba.CodigoAsync(respuesta));
            }

            var sinSesion = await EnviarAsync(_fabrica.CrearClienteDePrueba(), endpoint.Metodo, ruta);
            Assert.True(sinSesion.StatusCode == HttpStatusCode.Unauthorized, $"{endpoint} sin sesión: {(int)sinSesion.StatusCode}");
        }

        // Después de todo el recorrido, lo sembrado sigue exactamente igual.
        var detalle = await e.DetalleAsync(categoria.Id);
        Assert.True(detalle.GetProperty("activa").GetBoolean());
        Assert.Equal([equipo.Id], detalle.Lista("equipos").Ids("equipoId"));
        Assert.Equal("A", detalle.Lista("equipos").Single().GetProperty("nombre").GetString());
        Assert.Equal([jugador.Id], detalle.Lista("jugadores").Ids("usuarioRolId"));
        Assert.Equal([equipo.Id], detalle.Lista("jugadores").Single().Lista("equipos").Ids("equipoId"));
        var entrenador = Assert.Single(detalle.Lista("entrenadores"));
        Assert.Equal(e.IntegranteEntrenador.Id, entrenador.GetProperty("usuarioRolId").GetGuid());
        Assert.Equal([equipo.Id], entrenador.Lista("equipos").Ids("equipoId"));
        Assert.Single(await e.Presidente.ListaAsync(e.Categorias));
        Assert.Empty(await e.Presidente.ListaAsync(e.Retirados));
        Assert.Equal([e.IntegranteJugador.Id], (await e.Presidente.ListaAsync(e.SinCategoria)).Ids("usuarioRolId"));
    }

    [Fact]
    public async Task El_desarrollador_no_ve_ni_gestiona_las_categorias_de_ningun_club()
    {
        var e = await EscenarioCategorias.CrearAsync(_fabrica);
        var categoria = await e.Sembrar.CrearCategoriaAsync(e.Club, 2015);
        var desarrollador = await _fabrica.CrearClienteDePrueba().ConSesionDeDesarrolladorAsync();

        await AislamientoCategoriasPruebas.ComprobarAsync(desarrollador, HttpStatusCode.NotFound, "no_encontrado",
            ("GET", e.Categorias, null),
            ("GET", e.RutaCategoria(categoria.Id), null),
            ("GET", e.MiCategoria, null),
            ("GET", e.SinCategoria, null),
            ("POST", e.Categorias, "{\"anio\":2016}"),
            ("POST", e.Desactivacion(categoria.Id), null));
        Assert.Single(await e.Presidente.ListaAsync(e.Categorias));
    }

    private static Task<HttpResponseMessage> EnviarAsync(ClienteDePrueba cliente, string metodo, string ruta)
    {
        var peticion = new HttpRequestMessage(new HttpMethod(metodo), ruta);
        if (metodo is "POST" or "PUT")
        {
            peticion.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        }

        return cliente.Http.SendAsync(peticion);
    }
}
