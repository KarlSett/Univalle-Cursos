using Microsoft.AspNetCore.Mvc;

namespace Proyector_Web_3.Controllers;

public class HomeController : Controller
{
    // inicio = catalogo
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Cursos");
    }

    public IActionResult Error()
    {
        return View();
    }
}
