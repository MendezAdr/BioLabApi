using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BioLabApi.Models.DTOs
{
    public class RolUpdateDto
    {   
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El ID del rol debe ser un número positivo.")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "El nombre del rol no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un permiso para el rol.")]
        public List<RolModel.PermisosSistema> Permisos { get; set; } = new List<RolModel.PermisosSistema>();

    }
}