using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

public class CursosController : Controller
{
    private readonly PortalContext db;
    public CursosController(PortalContext db) { this.db = db; }

    public async Task<IActionResult> Index(string? buscar, string? categoria, string? nivel, string? orden)
    {
        var consulta = db.Cursos.AsNoTracking().Where(c => c.Publicado);
        int total = await consulta.CountAsync();
        buscar = buscar?.Trim();
        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(c => c.Titulo.Contains(buscar) || c.Descripcion.Contains(buscar));
        if (!string.IsNullOrEmpty(categoria)) consulta = consulta.Where(c => c.Categoria == categoria);
        if (!string.IsNullOrEmpty(nivel)) consulta = consulta.Where(c => c.Nivel == nivel);
        consulta = orden == "titulo" ? consulta.OrderBy(c => c.Titulo) : consulta.OrderByDescending(c => c.FechaCreacion).ThenBy(c => c.Id);
        var cursos = await consulta.Select(c => new TarjetaCurso
        {
            Id = c.Id, Titulo = c.Titulo, Descripcion = c.Descripcion, Nivel = c.Nivel,
            Categoria = c.Categoria, Portada = c.Portada, Lecciones = c.Lecciones.Count,
            Minutos = c.Lecciones.Sum(l => (int?)l.DuracionMinutos) ?? 0
        }).ToListAsync();
        return View(new CatalogoModelo { Cursos = cursos, Buscar = buscar, Categoria = categoria, Nivel = nivel, Orden = orden, TotalPublicados = total });
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var curso = await db.Cursos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.Publicado);
        if (curso == null) return NotFound();
        var modelo = new DetalleCursoModelo
        {
            Curso = curso,
            Lecciones = await db.Lecciones.Where(l => l.CursoId == id).OrderBy(l => l.Orden).ThenBy(l => l.Id)
                .Select(l => new ResumenLeccion { Id = l.Id, Titulo = l.Titulo, Orden = l.Orden, DuracionMinutos = l.DuracionMinutos }).ToListAsync()
        };
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Headers.CacheControl = "no-store";
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inscripcion = await db.Inscripciones.Include(i => i.LeccionesCompletadas)
                .FirstOrDefaultAsync(i => i.CursoId == id && i.UsuarioId == usuarioId);
            modelo.Inscrito = inscripcion != null;
            modelo.Completadas = inscripcion?.LeccionesCompletadas.Select(l => l.LeccionId).ToHashSet() ?? [];
        }
        return View(modelo);
    }

    [HttpPost, Authorize(Roles = "Suscriptor")]
    public async Task<IActionResult> Inscribirse(int id)
    {
        if (!await db.Cursos.AnyAsync(c => c.Id == id && c.Publicado && c.Lecciones.Any())) return NotFound();
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await db.Inscripciones.AnyAsync(i => i.CursoId == id && i.UsuarioId == usuarioId))
        {
            db.Inscripciones.Add(new Inscripcion { CursoId = id, UsuarioId = usuarioId });
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            { /* Dos envíos simultáneos conservan una única inscripción. */ }
        }
        TempData["Exito"] = "Ya estás inscrito. Puedes comenzar cuando quieras.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    [Authorize(Roles = "Suscriptor"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> MisCursos()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cursos = await db.Inscripciones.AsNoTracking().Where(i => i.UsuarioId == usuarioId)
            .OrderByDescending(i => i.FechaInscripcion).Select(i => new TarjetaCurso
            {
                Id = i.CursoId, Titulo = i.Curso.Titulo, Descripcion = i.Curso.Descripcion,
                Categoria = i.Curso.Categoria, Nivel = i.Curso.Nivel, Portada = i.Curso.Portada,
                Publicado = i.Curso.Publicado, Lecciones = i.Curso.Lecciones.Count,
                Minutos = i.Curso.Lecciones.Sum(l => (int?)l.DuracionMinutos) ?? 0,
                Completadas = i.LeccionesCompletadas.Count
            }).ToListAsync();
        return View(cursos);
    }
}
