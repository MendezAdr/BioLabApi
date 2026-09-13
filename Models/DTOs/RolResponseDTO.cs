using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace BioLabApi.Models.DTOs
{
    public class RolResponseDTO
    {
        public int Id { get; set; }
        public string RolName { get; set; } = string.Empty;
        public List<RolModel.PermisosSistema> Permisos { get; set; } = new List<RolModel.PermisosSistema>();
    }
}