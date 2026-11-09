namespace Proyector_Web_3.Models;

public class LeccionCompletada
{
    public int InscripcionId { get; set; }
    public Inscripcion Inscripcion { get; set; } = null!;
    public int LeccionId { get; set; }
    public Leccion Leccion { get; set; } = null!;
    public DateTime FechaCompletada { get; set; } = DateTime.UtcNow;
}
