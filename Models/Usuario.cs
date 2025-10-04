using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Portal_Academico.Models
{
    public class Usuario : IdentityUser
    {
        public string? Rol { get; set; }
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}