using System.ComponentModel.DataAnnotations;

namespace Proyector_Web_3.Models;

public class Curso
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(1200)]
    public string Descripcion { get; set; } = "";

    [Required, StringLength(30)]
    public string Categoria { get; set; } = "Desarrollo";

    [Required, StringLength(20)]
    public string Nivel { get; set; } = "Inicial";

    [Required, StringLength(20)]
    public string Portada { get; set; } = "web";

    public bool Publicado { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public List<Leccion> Lecciones { get; set; } = [];
    public List<Inscripcion> Inscripciones { get; set; } = [];
}
