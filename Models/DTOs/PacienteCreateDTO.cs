using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models.DTOs
{
    public class PacienteCreateDTO
    {
        
        [Required]
        [MaxLength(10)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [MaxLength(15)]
        public string Apellido { get; set; } = string.Empty;

        [Required] 
        [MaxLength(10)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        public DateTime FechaNacimiento { get; set; }
        [Required]
        public string Sexo { get; set; } = "N/A";
    

        [MaxLength(15)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(50)]
        public string NombreAcompañante { get; set; } = "N/A";

        [MaxLength(10)]
        public string CedulaAcompañante { get; set; } = "N/A";


    }
}
