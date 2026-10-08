using LaPecosa.Aplicacion.DTOs;
using LaPecosa.Aplicacion.Utilidades;
using LaPecosa.Dominio.Reglas;

namespace LaPecosa.Aplicacion.Validadores;

/// <summary>
/// Representa la validación de los datos del registro con invitación (constitución §12.1).
/// Su responsabilidad es exigir nombres, apellidos, tipo y número de documento, fecha de
/// nacimiento no futura, celular y una contraseña válida, y además el nombre del padre, madre o
/// responsable cuando la persona es menor de 18 años el día del registro (RF-010). Vale para todo
/// registro, también el de un presidente.
/// No valida el correo (sale de la invitación) ni comprueba si el documento ya existe.
/// </summary>
public static class ValidadorRegistro
{
    /// <summary>Longitud máxima del nombre del responsable.</summary>
    public const int MaximoResponsable = 160;

    /// <summary>
    /// Lanza <c>datos_invalidos</c> si los datos no son válidos. <paramref name="hoy"/> es la
    /// fecha UTC del servidor: con ella se rechazan las fechas futuras y se decide quién es menor.
    /// </summary>
    public static void Validar(RegistrarConInvitacionDto datos, DateOnly hoy)
    {
        var errores = new ErroresDeValidacion();

        errores.Obligatorio("nombres", datos.Nombres, 80, "El nombre");
        errores.Obligatorio("apellidos", datos.Apellidos, 80, "Los apellidos");
        errores.Obligatorio("celular", datos.Celular, 20, "El celular");

        if (datos.TipoDocumento is null || !Enum.IsDefined(datos.TipoDocumento.Value))
        {
            errores.Agregar("tipoDocumento", "Elige el tipo de documento.");
        }

        // La longitud se mide ya normalizado: sin espacios ni puntos, que no se guardan.
        var documento = NormalizadorTexto.Documento(datos.NumeroDocumento);
        if (documento.Length == 0)
        {
            errores.Agregar("numeroDocumento", "El número de documento es obligatorio.");
        }
        else if (documento.Length > 20)
        {
            errores.Agregar("numeroDocumento", "El número de documento admite como máximo 20 caracteres.");
        }

        if (datos.FechaNacimiento is null)
        {
            errores.Agregar("fechaNacimiento", "La fecha de nacimiento es obligatoria.");
        }
        else if (datos.FechaNacimiento > hoy)
        {
            errores.Agregar("fechaNacimiento", "La fecha de nacimiento no puede ser futura.");
        }
        else if (string.IsNullOrWhiteSpace(datos.NombreResponsable)
            && ReglaMayoriaDeEdad.EsMenorDeEdad(datos.FechaNacimiento.Value, hoy))
        {
            errores.Agregar(
                "nombreResponsable",
                "El nombre del padre, madre o responsable es obligatorio para una persona menor de 18 años.");
        }

        errores.Maximo("nombreResponsable", datos.NombreResponsable, MaximoResponsable, "El nombre del responsable");

        ValidadorContrasena.Validar(errores, "contrasena", datos.Contrasena);
        errores.LanzarSiHayErrores();
    }
}
