using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Data;
using Proyector_Web_3.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<PortalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Portal")));
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
}).AddEntityFrameworkStores<PortalContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Ingresar";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.Cookie.Name = "Univalle.Sesion";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

var app = builder.Build();

// Exporta el esquema a partir de los modelos, sin conectarse a SQL Server.
if (args.Contains("--exportar-sql"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PortalContext>();
    Directory.CreateDirectory("Database");
    var cabecera = "IF DB_ID(N'UnivalleMicrocursos') IS NULL\n    CREATE DATABASE [UnivalleMicrocursos];\nGO\nUSE [UnivalleMicrocursos];\nGO\n";
    await File.WriteAllTextAsync("Database/UnivalleMicrocursos.sql", cabecera + db.Database.GenerateCreateScript());
    return;
}

try
{
    using var scope = app.Services.CreateScope();
    await DatosIniciales.CrearAsync(scope.ServiceProvider, app.Configuration, app.Environment);
}
catch (SqlException ex)
{
    app.Logger.LogError("No se pudo preparar SQL Server. Revisa la conexión Portal y los permisos de Windows. Detalle: {Detalle}", ex.Message);
    Environment.ExitCode = 1;
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; style-src 'self'; script-src 'self'; img-src 'self' data:; font-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}");
app.MapControllerRoute("default", "{controller=Cursos}/{action=Index}/{id?}");
app.Run();
