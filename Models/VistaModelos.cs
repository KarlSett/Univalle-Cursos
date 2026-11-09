using System.ComponentModel.DataAnnotations;

namespace Proyector_Web_3.Models;

public static class OpcionesCurso
{
    public static readonly string[] Categorias = ["Desarrollo", "Diseño web", "Datos", "Herramientas"];
    public static readonly string[] Niveles = ["Inicial", "Intermedio"];
    public static readonly string[] Portadas = ["web", "sql", "git", "css", "csharp", "excel"];
}

public class TarjetaCurso
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Nivel { get; set; } = "";
    public string Portada { get; set; } = "";
    public int Lecciones { get; set; }
    public int Minutos { get; set; }
    public int Completadas { get; set; }
    public bool Publicado { get; set; }
    public int Inscritos { get; set; }
    public int Progreso => Lecciones == 0 ? 0 : Completadas * 100 / Lecciones;
}

public class CatalogoModelo
{
    public List<TarjetaCurso> Cursos { get; set; } = [];
    public string? Buscar { get; set; }
    public string? Categoria { get; set; }
    public string? Nivel { get; set; }
    public string? Orden { get; set; }
    public int TotalPublicados { get; set; }
}

public class ResumenLeccion
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public int Orden { get; set; }
    public int DuracionMinutos { get; set; }
}

public class DetalleCursoModelo
{
    public Curso Curso { get; set; } = null!;
    public List<ResumenLeccion> Lecciones { get; set; } = [];
    public HashSet<int> Completadas { get; set; } = [];
    public bool Inscrito { get; set; }
    public int Progreso => Lecciones.Count == 0 ? 0 : Completadas.Count * 100 / Lecciones.Count;
}

public class LecturaModelo
{
    public Leccion Leccion { get; set; } = null!;
    public List<ResumenLeccion> Lecciones { get; set; } = [];
    public HashSet<int> Completadas { get; set; } = [];
    public int Progreso => Lecciones.Count == 0 ? 0 : Completadas.Count * 100 / Lecciones.Count;
}

public class RegistroModelo
{
    [Required(ErrorMessage = "Escribe tu nombre."), StringLength(80), Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256), Display(Name = "Correo electrónico")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Escribe una contraseña."), StringLength(100, MinimumLength = 8, ErrorMessage = "Usa entre 8 y 100 caracteres."), DataType(DataType.Password), Display(Name = "Contraseña")]
    public string Password { get; set; } = "";
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden."), DataType(DataType.Password), Display(Name = "Repetir contraseña")]
    public string Confirmacion { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class LoginModelo
{
    [Required(ErrorMessage = "Escribe tu correo."), EmailAddress(ErrorMessage = "Escribe un correo válido."), StringLength(256), Display(Name = "Correo electrónico")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Escribe tu contraseña."), StringLength(100), DataType(DataType.Password), Display(Name = "Contraseña")]
    public string Password { get; set; } = "";
    [Display(Name = "Mantener mi sesión")]
    public bool Recordarme { get; set; }
    public string? ReturnUrl { get; set; }
}

public class CursoFormulario
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Escribe un título."), StringLength(80), Display(Name = "Título")]
    public string Titulo { get; set; } = "";
    [Required(ErrorMessage = "Escribe una descripción."), StringLength(1200), Display(Name = "Descripción")]
    public string Descripcion { get; set; } = "";
    [Required, Display(Name = "Categoría")]
    public string Categoria { get; set; } = "Desarrollo";
    [Required]
    public string Nivel { get; set; } = "Inicial";
    [Required]
    public string Portada { get; set; } = "web";
}

public class LeccionFormulario
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    [Required(ErrorMessage = "Escribe un título."), StringLength(100), Display(Name = "Título")]
    public string Titulo { get; set; } = "";
    [Required(ErrorMessage = "Escribe el contenido de la lección."), StringLength(20000)]
    public string Contenido { get; set; } = "";
    [Range(1, 180, ErrorMessage = "La duración debe estar entre 1 y 180 minutos."), Display(Name = "Duración en minutos")]
    public int DuracionMinutos { get; set; } = 10;
}
