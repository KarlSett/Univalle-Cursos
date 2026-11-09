using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CuentaController : Controller
{
    private readonly UserManager<Usuario> usuarios;
    private readonly SignInManager<Usuario> sesion;

    public CuentaController(UserManager<Usuario> usuarios, SignInManager<Usuario> sesion)
    {
        this.usuarios = usuarios;
        this.sesion = sesion;
    }

    [HttpGet]
    public IActionResult Ingresar(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Cursos");
        return View(new LoginModelo { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Ingresar(LoginModelo modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        var resultado = await sesion.PasswordSignInAsync(modelo.Email.Trim(), modelo.Password, modelo.Recordarme, lockoutOnFailure: true);
        if (resultado.Succeeded) return Volver(modelo.ReturnUrl);
        ModelState.AddModelError("", resultado.IsLockedOut
            ? "Demasiados intentos. Espera cinco minutos antes de volver a intentarlo."
            : "El correo o la contraseña no son correctos.");
        return View(modelo);
    }

    [HttpGet]
    public IActionResult Registro(string? returnUrl) => View(new RegistroModelo { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> Registro(RegistroModelo modelo)
    {
        if (!ModelState.IsValid) return View(modelo);
        var usuario = new Usuario { Nombre = modelo.Nombre.Trim(), Email = modelo.Email.Trim(), UserName = modelo.Email.Trim() };
        var resultado = await usuarios.CreateAsync(usuario, modelo.Password);
        if (resultado.Succeeded)
        {
            // El rol se asigna aquí; nunca se recibe desde el formulario de registro.
            var rol = await usuarios.AddToRoleAsync(usuario, "Suscriptor");
            if (!rol.Succeeded)
            {
                await usuarios.DeleteAsync(usuario);
                ModelState.AddModelError("", "No se pudo crear la cuenta. Inténtalo nuevamente.");
                return View(modelo);
            }
            await sesion.SignInAsync(usuario, isPersistent: false);
            TempData["Exito"] = "Tu cuenta está lista. Elige tu primer curso.";
            return Volver(modelo.ReturnUrl);
        }
        foreach (var error in resultado.Errors)
            ModelState.AddModelError("", error.Code.StartsWith("Duplicate")
                ? "Ya existe una cuenta con ese correo."
                : "Revisa tus datos. La contraseña necesita al menos 8 caracteres, una mayúscula, una minúscula y un número.");
        return View(modelo);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Salir()
    {
        await sesion.SignOutAsync();
        return RedirectToAction("Index", "Cursos");
    }

    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = 403;
        return View();
    }

    private IActionResult Volver(string? ruta)
    {
        if (Url.IsLocalUrl(ruta)) return LocalRedirect(ruta!);
        return RedirectToAction("Index", "Cursos");
    }
}
