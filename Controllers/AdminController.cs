using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

// solo admin
[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly PortalContext db;

    public AdminController(PortalContext db)
    {
        this.db = db;
    }

    // ----- cursos -----

    // lista todos los cursos
    public async Task<IActionResult> Index()
    {
        var cursos = await db.Cursos
            .Include(c => c.Lecciones)
            .Include(c => c.Inscripciones)
            .OrderBy(c => c.Titulo)
            .ToListAsync();

        return View(cursos);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View("Formulario", new CursoFormulario());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CursoFormulario modelo)
    {
        if (!ModelState.IsValid) return View("Formulario", modelo);

        // empieza como borrador
        var curso = new Curso
        {
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion,
            Categoria = modelo.Categoria,
            Nivel = modelo.Nivel
        };
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();

        TempData["Exito"] = "Curso creado. Ahora agrégale lecciones.";
        return RedirectToAction(nameof(Lecciones), new { id = curso.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        var modelo = new CursoFormulario
        {
            Id = curso.Id,
            Titulo = curso.Titulo,
            Descripcion = curso.Descripcion,
            Categoria = curso.Categoria,
            Nivel = curso.Nivel
        };
        return View("Formulario", modelo);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(CursoFormulario modelo)
    {
        if (!ModelState.IsValid) return View("Formulario", modelo);

        var curso = await db.Cursos.FindAsync(modelo.Id);
        if (curso == null) return NotFound();

        curso.Titulo = modelo.Titulo;
        curso.Descripcion = modelo.Descripcion;
        curso.Categoria = modelo.Categoria;
        curso.Nivel = modelo.Nivel;
        await db.SaveChangesAsync();

        TempData["Exito"] = "Curso actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();

        // primero las completadas, sql server no las borra en cascada
        var completadas = db.LeccionesCompletadas.Where(lc => lc.Leccion.CursoId == id);
        db.LeccionesCompletadas.RemoveRange(completadas);
        db.Cursos.Remove(curso);
        await db.SaveChangesAsync();

        TempData["Exito"] = "Curso eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // publicar / despublicar
    [HttpPost]
    public async Task<IActionResult> CambiarPublicacion(int id)
    {
        var curso = await db.Cursos.Include(c => c.Lecciones).FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();

        if (!curso.Publicado && curso.Lecciones.Count == 0)
        {
            TempData["Error"] = "Agrega al menos una lección antes de publicar.";
            return RedirectToAction(nameof(Index));
        }

        curso.Publicado = !curso.Publicado;
        await db.SaveChangesAsync();

        TempData["Exito"] = curso.Publicado ? "Curso publicado." : "Curso retirado del catálogo.";
        return RedirectToAction(nameof(Index));
    }

    // inscritos del curso
    public async Task<IActionResult> Inscritos(int id)
    {
        var curso = await db.Cursos
            .Include(c => c.Lecciones)
            .Include(c => c.Inscripciones).ThenInclude(i => i.Usuario)
            .Include(c => c.Inscripciones).ThenInclude(i => i.LeccionesCompletadas)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();

        return View(curso);
    }

    // ----- lecciones -----

    // lecciones del curso en orden
    public async Task<IActionResult> Lecciones(int id)
    {
        var curso = await db.Cursos
            .Include(c => c.Lecciones.OrderBy(l => l.Orden))
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();

        return View(curso);
    }

    [HttpGet]
    public IActionResult CrearLeccion(int cursoId)
    {
        return View("FormularioLeccion", new LeccionFormulario { CursoId = cursoId });
    }

    [HttpPost]
    public async Task<IActionResult> CrearLeccion(LeccionFormulario modelo)
    {
        if (!ModelState.IsValid) return View("FormularioLeccion", modelo);

        // va al final
        int cantidad = await db.Lecciones.CountAsync(l => l.CursoId == modelo.CursoId);

        db.Lecciones.Add(new Leccion
        {
            CursoId = modelo.CursoId,
            Titulo = modelo.Titulo,
            Contenido = modelo.Contenido,
            DuracionMinutos = modelo.DuracionMinutos,
            Orden = cantidad + 1
        });
        await db.SaveChangesAsync();

        TempData["Exito"] = "Lección agregada.";
        return RedirectToAction(nameof(Lecciones), new { id = modelo.CursoId });
    }

    [HttpGet]
    public async Task<IActionResult> EditarLeccion(int id)
    {
        var leccion = await db.Lecciones.FindAsync(id);
        if (leccion == null) return NotFound();

        var modelo = new LeccionFormulario
        {
            Id = leccion.Id,
            CursoId = leccion.CursoId,
            Titulo = leccion.Titulo,
            Contenido = leccion.Contenido,
            DuracionMinutos = leccion.DuracionMinutos
        };
        return View("FormularioLeccion", modelo);
    }

    [HttpPost]
    public async Task<IActionResult> EditarLeccion(LeccionFormulario modelo)
    {
        if (!ModelState.IsValid) return View("FormularioLeccion", modelo);

        var leccion = await db.Lecciones.FindAsync(modelo.Id);
        if (leccion == null) return NotFound();

        leccion.Titulo = modelo.Titulo;
        leccion.Contenido = modelo.Contenido;
        leccion.DuracionMinutos = modelo.DuracionMinutos;
        await db.SaveChangesAsync();

        TempData["Exito"] = "Lección actualizada.";
        return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
    }

    [HttpPost]
    public async Task<IActionResult> EliminarLeccion(int id)
    {
        var leccion = await db.Lecciones.FindAsync(id);
        if (leccion == null) return NotFound();

        // primero borrar las completadas
        db.LeccionesCompletadas.RemoveRange(db.LeccionesCompletadas.Where(lc => lc.LeccionId == id));
        db.Lecciones.Remove(leccion);
        await db.SaveChangesAsync();

        await Renumerar(leccion.CursoId);

        TempData["Exito"] = "Lección eliminada.";
        return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
    }

    // -1 sube, +1 baja (cambia orden con la vecina)
    [HttpPost]
    public async Task<IActionResult> MoverLeccion(int id, int direccion)
    {
        var leccion = await db.Lecciones.FindAsync(id);
        if (leccion == null) return NotFound();

        var vecina = await db.Lecciones.FirstOrDefaultAsync(l =>
            l.CursoId == leccion.CursoId && l.Orden == leccion.Orden + direccion);

        if (vecina != null)
        {
            vecina.Orden = leccion.Orden;
            leccion.Orden = leccion.Orden + direccion;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
    }

    // deja el orden 1, 2, 3... sin huecos
    private async Task Renumerar(int cursoId)
    {
        var lecciones = await db.Lecciones
            .Where(l => l.CursoId == cursoId)
            .OrderBy(l => l.Orden)
            .ToListAsync();

        for (int i = 0; i < lecciones.Count; i++)
            lecciones[i].Orden = i + 1;

        await db.SaveChangesAsync();
    }
}
