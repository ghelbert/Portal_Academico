using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal_Academico.Models;

namespace Portal_Academico.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario,Rol,string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Curso> Cursos {get; set;}
    public DbSet<Matricula> Matriculas {get; set;}
}
