using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Cursos");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Acerca() => View();

        public IActionResult Estado(int codigo)
        {
            Response.StatusCode = codigo;
            return View(codigo);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
