using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

var builder = WebApplication.CreateBuilder(args);

// mvc + token antifalsificacion en todos los post
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// ef core con sql server (cadena en appsettings.json)
builder.Services.AddDbContext<PortalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Portal")));

// identity: usuarios y roles en las tablas aspnet*
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<PortalContext>();

// rutas de login y acceso denegado
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Ingresar";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
});

var app = builder.Build();

// crea la base, roles, admin y cursos de ejemplo
using (var scope = app.Services.CreateScope())
{
    await DatosIniciales.CrearAsync(scope.ServiceProvider, app.Configuration, app.Environment);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // quien es
app.UseAuthorization();  // tiene permiso

app.MapControllerRoute("default", "{controller=Cursos}/{action=Index}/{id?}");
app.Run();
