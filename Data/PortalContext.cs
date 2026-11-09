using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proyector_Web_3.Models;

namespace Proyector_Web_3.Data;

public class PortalContext : IdentityDbContext<Usuario>
{
    public PortalContext(DbContextOptions<PortalContext> options) : base(options) { }

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Leccion> Lecciones => Set<Leccion>();
    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();
    public DbSet<LeccionCompletada> LeccionesCompletadas => Set<LeccionCompletada>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserLogin<string>>().Property(l => l.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserLogin<string>>().Property(l => l.ProviderKey).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(t => t.LoginProvider).HasMaxLength(128);
        builder.Entity<IdentityUserToken<string>>().Property(t => t.Name).HasMaxLength(128);

        // Una persona solo puede inscribirse una vez en el mismo curso.
        builder.Entity<Inscripcion>().HasIndex(i => new { i.UsuarioId, i.CursoId }).IsUnique();
        builder.Entity<Leccion>().HasIndex(l => new { l.CursoId, l.Orden });
        builder.Entity<LeccionCompletada>().HasKey(l => new { l.InscripcionId, l.LeccionId });

        // Evita dos rutas de eliminación en cascada hacia la misma tabla en SQL Server.
        builder.Entity<LeccionCompletada>().HasOne(l => l.Leccion).WithMany()
            .HasForeignKey(l => l.LeccionId).OnDelete(DeleteBehavior.NoAction);
    }
}
