using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

public class CursosController : Controller
{
    private readonly PortalContext db;

    public CursosController(PortalContext db)
    {
        this.db = db;
    }

    // catalogo publico
    public async Task<IActionResult> Index()
    {
        var cursos = await db.Cursos
            .Include(c => c.Lecciones)
            .Where(c => c.Publicado)
            .OrderBy(c => c.Titulo)
            .ToListAsync();

        return View(cursos);
    }

    // presentacion del curso
    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await db.Cursos
            .Include(c => c.Lecciones.OrderBy(l => l.Orden))
            .FirstOrDefaultAsync(c => c.Id == id);

        // borrador solo lo ve el admin
        if (curso == null || (!curso.Publicado && !User.IsInRole("Administrador")))
            return NotFound();

        var modelo = new DetalleCursoModelo { Curso = curso };

        // si esta inscrito y que completo
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var inscripcion = await db.Inscripciones
            .Include(i => i.LeccionesCompletadas)
            .FirstOrDefaultAsync(i => i.CursoId == id && i.UsuarioId == usuarioId);

        if (inscripcion != null)
        {
            modelo.Inscrito = true;
            modelo.Completadas = inscripcion.LeccionesCompletadas.Select(lc => lc.LeccionId).ToList();
        }

        return View(modelo);
    }

    // inscribirse (solo suscriptor)
    [HttpPost]
    [Authorize(Roles = "Suscriptor")]
    public async Task<IActionResult> Inscribirse(int id)
    {
        var curso = await db.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Publicado);
        if (curso == null) return NotFound();

        // id desde la sesion
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        bool yaInscrito = await db.Inscripciones.AnyAsync(i => i.CursoId == id && i.UsuarioId == usuarioId);
        if (!yaInscrito)
        {
            db.Inscripciones.Add(new Inscripcion { CursoId = id, UsuarioId = usuarioId });
            await db.SaveChangesAsync();
        }

        TempData["Exito"] = "Te inscribiste en el curso.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // mis cursos con progreso
    [Authorize(Roles = "Suscriptor")]
    public async Task<IActionResult> MisCursos()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var inscripciones = await db.Inscripciones
            .Include(i => i.Curso).ThenInclude(c => c.Lecciones)
            .Include(i => i.LeccionesCompletadas)
            .Where(i => i.UsuarioId == usuarioId)
            .ToListAsync();

        var modelo = inscripciones.Select(i => new MiCursoModelo
        {
            Curso = i.Curso,
            Completadas = i.LeccionesCompletadas.Count,
            Total = i.Curso.Lecciones.Count
        }).ToList();

        return View(modelo);
    }
}
