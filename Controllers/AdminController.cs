using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

[Authorize(Roles = "Administrador")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AdminController : Controller
{
    private readonly PortalContext db;
    public AdminController(PortalContext db) { this.db = db; }

    public async Task<IActionResult> Index()
    {
        var cursos = await db.Cursos.AsNoTracking().OrderByDescending(c => c.FechaCreacion)
            .Select(c => new TarjetaCurso
            {
                Id = c.Id, Titulo = c.Titulo, Categoria = c.Categoria, Nivel = c.Nivel,
                Portada = c.Portada, Publicado = c.Publicado, Lecciones = c.Lecciones.Count,
                Inscritos = c.Inscripciones.Count, Minutos = c.Lecciones.Sum(l => (int?)l.DuracionMinutos) ?? 0
            }).ToListAsync();
        return View(cursos);
    }

    [HttpGet]
    public IActionResult Crear() => View("Formulario", new CursoFormulario());

    [HttpPost]
    public async Task<IActionResult> Crear(CursoFormulario modelo)
    {
        ValidarOpciones(modelo);
        if (!ModelState.IsValid) return View("Formulario", modelo);
        var curso = new Curso();
        CopiarDatos(modelo, curso);
        db.Cursos.Add(curso);
        await db.SaveChangesAsync();
        TempData["Exito"] = "Curso creado como borrador. Agrega sus lecciones para publicarlo.";
        return RedirectToAction(nameof(Lecciones), new { id = curso.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        return View("Formulario", new CursoFormulario
        {
            Id = curso.Id, Titulo = curso.Titulo, Descripcion = curso.Descripcion,
            Categoria = curso.Categoria, Nivel = curso.Nivel, Portada = curso.Portada
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(int id, CursoFormulario modelo)
    {
        if (id != modelo.Id) return BadRequest();
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        ValidarOpciones(modelo);
        if (!ModelState.IsValid) return View("Formulario", modelo);
        CopiarDatos(modelo, curso);
        await db.SaveChangesAsync();
        TempData["Exito"] = "Los cambios del curso se guardaron.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Publicar(int id)
    {
        var curso = await db.Cursos.Include(c => c.Lecciones).FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();
        if (!curso.Publicado && curso.Lecciones.Count == 0)
            TempData["Error"] = "Agrega al menos una lección antes de publicar el curso.";
        else
        {
            curso.Publicado = !curso.Publicado;
            await db.SaveChangesAsync();
            TempData["Exito"] = curso.Publicado ? "El curso ya aparece en el catálogo." : "Curso retirado. Las inscripciones y el progreso se conservan.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        using var transaccion = await db.Database.BeginTransactionAsync();
        var completadas = await db.LeccionesCompletadas.Where(l => l.Inscripcion.CursoId == id).ToListAsync();
        db.LeccionesCompletadas.RemoveRange(completadas);
        await db.SaveChangesAsync();
        db.Cursos.Remove(curso);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        TempData["Exito"] = "Curso eliminado junto con sus lecciones, inscripciones y progreso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Lecciones(int id)
    {
        var curso = await db.Cursos.AsNoTracking().Include(c => c.Lecciones).FirstOrDefaultAsync(c => c.Id == id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpGet]
    public async Task<IActionResult> CrearLeccion(int cursoId)
    {
        var curso = await db.Cursos.FindAsync(cursoId);
        if (curso == null) return NotFound();
        ViewData["CursoTitulo"] = curso.Titulo;
        return View("FormularioLeccion", new LeccionFormulario { CursoId = cursoId });
    }

    [HttpPost]
    public async Task<IActionResult> CrearLeccion(LeccionFormulario modelo)
    {
        var curso = await db.Cursos.FindAsync(modelo.CursoId);
        if (curso == null) return NotFound();
        ViewData["CursoTitulo"] = curso.Titulo;
        if (!ModelState.IsValid) return View("FormularioLeccion", modelo);
        var ultimoOrden = await db.Lecciones.Where(l => l.CursoId == curso.Id).MaxAsync(l => (int?)l.Orden) ?? 0;
        db.Lecciones.Add(new Leccion
        {
            CursoId = curso.Id, Titulo = modelo.Titulo.Trim(), Contenido = modelo.Contenido.Trim(),
            DuracionMinutos = modelo.DuracionMinutos, Orden = ultimoOrden + 1
        });
        await db.SaveChangesAsync();
        TempData["Exito"] = "Lección agregada.";
        return RedirectToAction(nameof(Lecciones), new { id = curso.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditarLeccion(int id)
    {
        var leccion = await db.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();
        ViewData["CursoTitulo"] = leccion.Curso.Titulo;
        return View("FormularioLeccion", new LeccionFormulario
        {
            Id = id, CursoId = leccion.CursoId, Titulo = leccion.Titulo,
            Contenido = leccion.Contenido, DuracionMinutos = leccion.DuracionMinutos
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditarLeccion(int id, LeccionFormulario modelo)
    {
        if (id != modelo.Id) return BadRequest();
        var leccion = await db.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null || leccion.CursoId != modelo.CursoId) return NotFound();
        ViewData["CursoTitulo"] = leccion.Curso.Titulo;
        if (!ModelState.IsValid) return View("FormularioLeccion", modelo);
        leccion.Titulo = modelo.Titulo.Trim();
        leccion.Contenido = modelo.Contenido.Trim();
        leccion.DuracionMinutos = modelo.DuracionMinutos;
        await db.SaveChangesAsync();
        TempData["Exito"] = "Lección actualizada.";
        return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
    }

    [HttpPost]
    public async Task<IActionResult> MoverLeccion(int id, int direccion)
    {
        if (direccion != -1 && direccion != 1) return BadRequest();
        var actual = await db.Lecciones.FindAsync(id);
        if (actual == null) return NotFound();
        var lecciones = await db.Lecciones.Where(l => l.CursoId == actual.CursoId).OrderBy(l => l.Orden).ThenBy(l => l.Id).ToListAsync();
        int posicion = lecciones.FindIndex(l => l.Id == id);
        int destino = posicion + direccion;
        if (destino >= 0 && destino < lecciones.Count)
        {
            (lecciones[posicion], lecciones[destino]) = (lecciones[destino], lecciones[posicion]);
            for (int i = 0; i < lecciones.Count; i++) lecciones[i].Orden = i + 1;
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Lecciones), new { id = actual.CursoId });
    }

    [HttpPost]
    public async Task<IActionResult> EliminarLeccion(int id)
    {
        var leccion = await db.Lecciones.Include(l => l.Curso).FirstOrDefaultAsync(l => l.Id == id);
        if (leccion == null) return NotFound();
        using var transaccion = await db.Database.BeginTransactionAsync();
        db.LeccionesCompletadas.RemoveRange(await db.LeccionesCompletadas.Where(l => l.LeccionId == id).ToListAsync());
        await db.SaveChangesAsync();
        db.Lecciones.Remove(leccion);
        var restantes = await db.Lecciones.Where(l => l.CursoId == leccion.CursoId && l.Id != id).OrderBy(l => l.Orden).ThenBy(l => l.Id).ToListAsync();
        for (int i = 0; i < restantes.Count; i++) restantes[i].Orden = i + 1;
        if (restantes.Count == 0) leccion.Curso.Publicado = false;
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        TempData["Exito"] = "Lección eliminada. El progreso se calcula con las lecciones restantes.";
        return RedirectToAction(nameof(Lecciones), new { id = leccion.CursoId });
    }

    public async Task<IActionResult> Inscritos(int id)
    {
        var curso = await db.Cursos.FindAsync(id);
        if (curso == null) return NotFound();
        ViewData["CursoTitulo"] = curso.Titulo;
        ViewData["CursoId"] = curso.Id;
        ViewData["TotalLecciones"] = await db.Lecciones.CountAsync(l => l.CursoId == id);
        return View(await db.Inscripciones.AsNoTracking().Include(i => i.Usuario).Include(i => i.LeccionesCompletadas)
            .Where(i => i.CursoId == id).OrderByDescending(i => i.FechaInscripcion).ToListAsync());
    }

    private void ValidarOpciones(CursoFormulario modelo)
    {
        if (!OpcionesCurso.Categorias.Contains(modelo.Categoria)) ModelState.AddModelError(nameof(modelo.Categoria), "Selecciona una categoría válida.");
        if (!OpcionesCurso.Niveles.Contains(modelo.Nivel)) ModelState.AddModelError(nameof(modelo.Nivel), "Selecciona un nivel válido.");
        if (!OpcionesCurso.Portadas.Contains(modelo.Portada)) ModelState.AddModelError(nameof(modelo.Portada), "Selecciona una portada válida.");
    }

    private static void CopiarDatos(CursoFormulario origen, Curso destino)
    {
        destino.Titulo = origen.Titulo.Trim();
        destino.Descripcion = origen.Descripcion.Trim();
        destino.Categoria = origen.Categoria;
        destino.Nivel = origen.Nivel;
        destino.Portada = origen.Portada;
    }
}
