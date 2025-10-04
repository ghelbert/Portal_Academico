using System;

namespace Portal_Academico.Models
{
    // DTO usado solo para cachear una proyección ligera de Curso
    public class CursoCacheDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int Creditos { get; set; }
        public int CupoMaximo { get; set; }
        public string? HorarioInicio { get; set; }
        public string? HorarioFin { get; set; }
    }
}
