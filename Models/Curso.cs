using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Portal_Academico.Models
{
    public class Curso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Codigo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayores a 0.")]
        public int Creditos { get; set; }

        [Range(1, 1000)]
        public int CupoMaximo { get; set; }

        public string HorarioInicio { get; set; }

        public string HorarioFin { get; set; }

        public string Activo { get; set; }

        public ICollection<Matricula> Matriculas { get; set; }

       

    }

    }