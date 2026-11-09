using System.ComponentModel.DataAnnotations;

namespace Proyector_Web_3.Models;

public class Leccion
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    [Required, StringLength(100)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(20000)]
    public string Contenido { get; set; } = "";

    [Range(1, 180)]
    public int DuracionMinutos { get; set; } = 10;

    public int Orden { get; set; }
}
