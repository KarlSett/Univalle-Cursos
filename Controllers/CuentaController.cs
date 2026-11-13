using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

public class CuentaController : Controller
{
    // usermanager: usuarios y roles, signinmanager: sesion
    private readonly UserManager<Usuario> userManager;
    private readonly SignInManager<Usuario> signInManager;

    public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Ingresar(string? returnUrl)
    {
        return View(new LoginModelo { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Ingresar(LoginModelo modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, false, false);
        if (!resultado.Succeeded)
        {
            ModelState.AddModelError("", "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        // vuelve a la pagina anterior si es local
        if (Url.IsLocalUrl(modelo.ReturnUrl)) return Redirect(modelo.ReturnUrl);
        return RedirectToAction("Index", "Cursos");
    }

    [HttpGet]
    public IActionResult Registro()
    {
        return View(new RegistroModelo());
    }

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroModelo modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var usuario = new Usuario { Nombre = modelo.Nombre, Email = modelo.Email, UserName = modelo.Email };
        var resultado = await userManager.CreateAsync(usuario, modelo.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);
            return View(modelo);
        }

        // el rol lo pone el servidor
        await userManager.AddToRoleAsync(usuario, "Suscriptor");
        await signInManager.SignInAsync(usuario, false);

        TempData["Exito"] = "Cuenta creada. Ya puedes inscribirte en los cursos.";
        return RedirectToAction("Index", "Cursos");
    }

    [HttpPost]
    public async Task<IActionResult> Salir()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Cursos");
    }

    // cuando no tiene permiso
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
