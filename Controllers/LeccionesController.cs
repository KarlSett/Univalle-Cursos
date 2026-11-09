using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

[Authorize(Roles = "Suscriptor")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LeccionesController : Controller
{
    private readonly PortalContext db;
    public LeccionesController(PortalContext db) { this.db = db; }

    public async Task<IActionResult> Leer(int id)
    {
        // Solo consultamos el identificador del curso antes de verificar el permiso.
        var cursoId = await db.Lecciones.Where(l => l.Id == id).Select(l => (int?)l.CursoId).FirstOrDefaultAsync();
        if (cursoId == null) return NotFound();
        var inscripcion = await BuscarInscripcion(cursoId.Value);
        if (inscripcion == null) return Forbid();

        // El contenido se recupera únicamente después de comprobar la inscripción en el servidor.
        var leccion = await db.Lecciones.AsNoTracking().Include(l => l.Curso).FirstAsync(l => l.Id == id);
        var modelo = new LecturaModelo
        {
            Leccion = leccion,
            Lecciones = await db.Lecciones.Where(l => l.CursoId == leccion.CursoId).OrderBy(l => l.Orden).ThenBy(l => l.Id)
                .Select(l => new ResumenLeccion { Id = l.Id, Titulo = l.Titulo, Orden = l.Orden, DuracionMinutos = l.DuracionMinutos }).ToListAsync(),
            Completadas = (await db.LeccionesCompletadas.Where(l => l.InscripcionId == inscripcion.Id).Select(l => l.LeccionId).ToListAsync()).ToHashSet()
        };
        return View(modelo);
    }

    [HttpPost]
    public async Task<IActionResult> Completar(int id)
    {
        var cursoId = await db.Lecciones.Where(l => l.Id == id).Select(l => (int?)l.CursoId).FirstOrDefaultAsync();
        if (cursoId == null) return NotFound();
        var inscripcion = await BuscarInscripcion(cursoId.Value);
        if (inscripcion == null) return Forbid();

        if (!await db.LeccionesCompletadas.AnyAsync(l => l.InscripcionId == inscripcion.Id && l.LeccionId == id))
        {
            db.LeccionesCompletadas.Add(new LeccionCompletada { InscripcionId = inscripcion.Id, LeccionId = id });
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            { /* El índice impide duplicar una lección completada. */ }
        }
        TempData["Exito"] = "Lección completada. Tu progreso se ha guardado.";
        return RedirectToAction(nameof(Leer), new { id });
    }

    private Task<Inscripcion?> BuscarInscripcion(int cursoId)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return db.Inscripciones.FirstOrDefaultAsync(i =>
            i.UsuarioId == usuarioId && i.CursoId == cursoId && i.Curso.Publicado);
    }
}
