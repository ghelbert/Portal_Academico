using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Portal_Academico.Models
{
    public class Filtro : PageModel
    {
        public string NombreFiltro { get; set; }
        public int? MinCreditos { get; set; }
        public int? MaxCreditos { get; set; }
        public TimeSpan? HorarioInicioFiltro { get; set; }
        public TimeSpan? HorarioFinFiltro { get; set; }
        public List<Curso> Cursos { get; set; }

        // public void OnGet()
        // {
        //     var query = dbContext.Cursos.AsQueryable();
        //     if (!string.IsNullOrEmpty(NombreFiltro))
        //         query = query.Where(c => c.Nombre.Contains(NombreFiltro));
        //     if (MinCreditos.HasValue)
        //         query = query.Where(c => c.Creditos >= MinCreditos.Value);
        //     if (MaxCreditos.HasValue)
        //         query = query.Where(c => c.Creditos <= MaxCreditos.Value);
        //     if (HorarioInicioFiltro.HasValue)
        //         query = query.Where(c => c.HorarioInicio >= HorarioInicioFiltro.Value);
        //     if (HorarioFinFiltro.HasValue)
        //         query = query.Where(c => c.HorarioFin <= HorarioFinFiltro.Value);

        //     Cursos = query.ToList();
        // }
    }
}