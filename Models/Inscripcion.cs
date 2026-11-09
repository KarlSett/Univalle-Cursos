namespace Proyector_Web_3.Models;

public class Inscripcion
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;
    public string UsuarioId { get; set; } = "";
    public Usuario Usuario { get; set; } = null!;
    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
    public List<LeccionCompletada> LeccionesCompletadas { get; set; } = [];
}
