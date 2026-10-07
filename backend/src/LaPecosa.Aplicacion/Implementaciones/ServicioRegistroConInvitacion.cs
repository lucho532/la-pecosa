using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Interfaces;
using LaPecosa.Aplicacion.Mappers;
using LaPecosa.Aplicacion.Servicios;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Aplicacion.Validadores;
using LaPecosa.Dominio.Entidades;
using LaPecosa.Dominio.Enumeraciones;

namespace LaPecosa.Aplicacion.Implementaciones;

/// <summary>
/// Representa el servicio de registro con invitación.
/// Su responsabilidad es comprobar que la invitación está vigente, que el correo no tiene cuenta y
/// que el documento no se repite en el club ni pertenece a otra cuenta (una persona tiene un único
/// inicio de sesión), y crear la cuenta y el integrante en la misma transacción en que la
/// invitación queda usada.
/// No acepta un correo, un club ni un rol enviados por quien se registra, y no puede crear una
/// cuenta DESARROLLADOR. No accede al contexto de Entity Framework ni conoce HTTP.
/// </summary>
public class ServicioRegistroConInvitacion : IServicioRegistroConInvitacion
{
    private readonly IRepositorioInvitacionesPorToken _invitaciones;
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioPertenencias _pertenencias;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IHashContrasena _hash;
    private readonly IEmisorTokenSesion _emisor;
    private readonly IReloj _reloj;

    /// <summary>Crea el servicio con sus dependencias.</summary>
    public ServicioRegistroConInvitacion(
        IRepositorioInvitacionesPorToken invitaciones,
        IRepositorioUsuarios usuarios,
        IRepositorioPertenencias pertenencias,
        IUnidadDeTrabajo unidadDeTrabajo,
        IHashContrasena hash,
        IEmisorTokenSesion emisor,
        IReloj reloj)
    {
        _invitaciones = invitaciones;
        _usuarios = usuarios;
        _pertenencias = pertenencias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _hash = hash;
        _emisor = emisor;
        _reloj = reloj;
    }

    /// <inheritdoc />
    public async Task<InvitacionVigenteDto> ConsultarAsync(TokenDto datos, CancellationToken cancelacion = default)
    {
        var invitacion = await VigenteAsync(datos.Token, cancelacion);
        var tieneCuenta = await _usuarios.ObtenerPorCorreoAsync(invitacion.Correo, cancelacion) is not null;

        return new InvitacionVigenteDto(
            invitacion.Club!.Nombre,
            invitacion.Rol,
            invitacion.Correo,
            tieneCuenta,
            MapperIdentidadClub.AIdentidad(invitacion.Club));
    }

    /// <inheritdoc />
    public async Task<TokenSesionDto> RegistrarAsync(
        RegistrarConInvitacionDto datos, CancellationToken cancelacion = default)
    {
        var invitacion = await VigenteAsync(datos.Token, cancelacion);
        var ahora = _reloj.AhoraUtc;
        ValidadorRegistro.Validar(datos, DateOnly.FromDateTime(ahora));

        var documento = NormalizadorTexto.Documento(datos.NumeroDocumento);
        if (await _usuarios.ObtenerPorCorreoAsync(invitacion.Correo, cancelacion) is not null)
        {
            throw ErroresDeInvitacion.CorreoYaRegistrado();
        }

        if (await _pertenencias.ExisteDocumentoEnClubAsync(invitacion.ClubId, documento, cancelacion))
        {
            throw ErroresDeInvitacion.DocumentoRepetidoEnClub();
        }

        if (await _pertenencias.ObtenerCuentaPorDocumentoAsync(documento, cancelacion) is not null)
        {
            throw ExcepcionDeAplicacion.Conflicto(
                "documento_en_otra_cuenta",
                "Ese documento ya está registrado con otra cuenta. Inicia sesión con esa cuenta para aceptar la invitación.");
        }

        // El correo es siempre el de la invitación (RF-013); el club y el rol, también.
        var usuario = new Usuario
        {
            Correo = invitacion.Correo,
            CorreoNormalizado = invitacion.Correo,
            ContrasenaHash = _hash.Calcular(datos.Contrasena!),
            Celular = datos.Celular!.Trim(),
            EsDesarrollador = false,
            CreadoEn = ahora,
        };
        var integrante = new UsuarioRol
        {
            ClubId = invitacion.ClubId,
            UsuarioId = usuario.Id,
            Rol = invitacion.Rol,
            EstadoIngreso = EstadoIngreso.APROBADO,
            Nombres = NormalizadorTexto.SinEspaciosSobrantes(datos.Nombres),
            Apellidos = NormalizadorTexto.SinEspaciosSobrantes(datos.Apellidos),
            TipoDocumento = datos.TipoDocumento!.Value,
            NumeroDocumento = documento,
            FechaNacimiento = datos.FechaNacimiento!.Value,
            CreadoEn = ahora,
        };

        try
        {
            await _unidadDeTrabajo.EnTransaccionAsync(
                async () =>
                {
                    if (!await _invitaciones.MarcarUsadaAsync(invitacion.Id, ahora, cancelacion))
                    {
                        throw ErroresDeInvitacion.NoValida();
                    }

                    _usuarios.Agregar(usuario);
                    _pertenencias.Agregar(integrante);
                    await _unidadDeTrabajo.GuardarAsync(cancelacion);
                },
                cancelacion);
        }
        catch (Exception error) when (_unidadDeTrabajo.EsViolacionDeUnicidad(error, out var indice)
            && ErroresDeInvitacion.DeIndiceUnico(indice) is { } conflicto)
        {
            // Dos registros simultáneos: el índice único decide y se responde el mismo 409.
            throw conflicto;
        }

        return MapperSesion.AToken(_emisor.Emitir(usuario.Id, usuario.SelloSeguridad));
    }

    private async Task<Invitacion> VigenteAsync(string? token, CancellationToken cancelacion)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw ErroresDeInvitacion.NoValida();
        }

        var invitacion = await _invitaciones.ObtenerPorHashAsync(GeneradorTokens.Hash(token), cancelacion);
        if (invitacion?.Club is null || !invitacion.EstaVigente(_reloj.AhoraUtc))
        {
            throw ErroresDeInvitacion.NoValida();
        }

        return invitacion;
    }
}
