using System;
using System.Collections.Generic;

namespace Portal_Academico.Models
{
    // ViewModel simple para la vista de catálogo (POCO para MVC)
    public class CatalogoViewModel
    {
    public string? NombreFiltro { get; set; }
        public int? MinCreditos { get; set; }
        public int? MaxCreditos { get; set; }
        public TimeSpan? HorarioInicioFiltro { get; set; }
        public TimeSpan? HorarioFinFiltro { get; set; }
        public List<Curso> Cursos { get; set; } = new List<Curso>();
    public string? ErrorMessage { get; set; }
    }
}