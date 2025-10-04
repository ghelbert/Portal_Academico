using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal_Academico.Models;

namespace Portal_Academico.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

      
        // Configuración inicial de los cursos
        modelBuilder.Entity<Curso>().HasData(
            new Curso
            {
                Id = 1,
                Codigo = "MATH101",
                Nombre = "Matematica",
                Creditos = 4,
                CupoMaximo = 30,
                HorarioInicio = "10:00", // Formato "HH:mm"
                HorarioFin = "12:00",    // Formato "HH:mm"
                Activo = "true"
            },
            new Curso
            {
                Id = 2,
                Codigo = "PHYS101",
                Nombre = "Fisica",
                Creditos = 3,
                CupoMaximo = 25,
                HorarioInicio = "11:00", // Formato "HH:mm"
                HorarioFin = "13:00",    // Formato "HH:mm"
                Activo = "true"
            },
            new Curso
            {
                Id = 3,
                Codigo = "CHEM101",
                Nombre = "Quimica",
                Creditos = 5,
                CupoMaximo = 20,
                HorarioInicio = "12:00", // Formato "HH:mm"
                HorarioFin = "14:00",    // Formato "HH:mm"
                Activo = "true"
            }
        );



    }
    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
}
