using System.ComponentModel.DataAnnotations;

namespace Proyector_Web_3.Models;

// no son tablas, solo para pasar datos a las vistas

public static class OpcionesCurso
{
    public static readonly string[] Categorias = ["Desarrollo", "Diseño web", "Datos", "Herramientas"];
    public static readonly string[] Niveles = ["Inicial", "Intermedio"];
}

// detalle del curso
public class DetalleCursoModelo
{
    public Curso Curso { get; set; } = null!;
    public bool Inscrito { get; set; }
    public List<int> Completadas { get; set; } = [];
}

// leer leccion
public class LeerLeccionModelo
{
    public Leccion Leccion { get; set; } = null!;
    public List<Leccion> Lecciones { get; set; } = [];
    public List<int> Completadas { get; set; } = [];
}

// mis cursos
public class MiCursoModelo
{
    public Curso Curso { get; set; } = null!;
    public int Completadas { get; set; }
    public int Total { get; set; }
    public int Porcentaje => Total == 0 ? 0 : Completadas * 100 / Total;
}

// form de curso
public class CursoFormulario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Escribe un título."), StringLength(80)]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Escribe una descripción."), StringLength(1200)]
    public string Descripcion { get; set; } = "";

    public string Categoria { get; set; } = "Desarrollo";
    public string Nivel { get; set; } = "Inicial";
}

// form de leccion
public class LeccionFormulario
{
    public int Id { get; set; }
    public int CursoId { get; set; }

    [Required(ErrorMessage = "Escribe un título."), StringLength(100)]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "Escribe el contenido."), StringLength(20000)]
    public string Contenido { get; set; } = "";

    [Range(1, 180, ErrorMessage = "La duración debe estar entre 1 y 180 minutos.")]
    public int DuracionMinutos { get; set; } = 10;
}

public class RegistroModelo
{
    [Required(ErrorMessage = "Escribe tu nombre.")]
    [StringLength(80)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Escribe una contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    public string Confirmacion { get; set; } = "";
}

public class LoginModelo
{
    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
