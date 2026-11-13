using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

// solo suscriptores
[Authorize(Roles = "Suscriptor")]
public class LeccionesController : Controller
{
    private readonly PortalContext db;

    public LeccionesController(PortalContext db)
    {
        this.db = db;
    }

    // leer leccion
    public async Task<IActionResult> Leer(int id)
    {
        var leccion = await db.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();

        // verificacion en el servidor, aunque pongan la url a mano
        var inscripcion = await BuscarInscripcion(leccion.CursoId);
        if (inscripcion == null) return Forbid();

        var modelo = new LeerLeccionModelo
        {
            Leccion = leccion,
            Lecciones = await db.Lecciones
                .Where(l => l.CursoId == leccion.CursoId)
                .OrderBy(l => l.Orden)
                .ToListAsync(),
            Completadas = await db.LeccionesCompletadas
                .Where(lc => lc.InscripcionId == inscripcion.Id)
                .Select(lc => lc.LeccionId)
                .ToListAsync()
        };

        return View(modelo);
    }

    // marcar como completada
    [HttpPost]
    public async Task<IActionResult> Completar(int id)
    {
        var leccion = await db.Lecciones.FindAsync(id);
        if (leccion == null) return NotFound();

        // misma verificacion
        var inscripcion = await BuscarInscripcion(leccion.CursoId);
        if (inscripcion == null) return Forbid();

        bool yaCompletada = await db.LeccionesCompletadas
            .AnyAsync(lc => lc.InscripcionId == inscripcion.Id && lc.LeccionId == id);

        if (!yaCompletada)
        {
            db.LeccionesCompletadas.Add(new LeccionCompletada { InscripcionId = inscripcion.Id, LeccionId = id });
            await db.SaveChangesAsync();
        }

        TempData["Exito"] = "Lección marcada como completada.";
        return RedirectToAction(nameof(Leer), new { id });
    }

    // inscripcion del usuario en el curso (null si no hay)
    // el curso tiene que estar publicado
    private async Task<Inscripcion?> BuscarInscripcion(int cursoId)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return await db.Inscripciones.FirstOrDefaultAsync(i =>
            i.UsuarioId == usuarioId &&
            i.CursoId == cursoId &&
            i.Curso.Publicado);
    }
}
