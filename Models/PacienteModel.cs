
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioLabApi.Models;
public class PacienteModel : Auditable
{
    /*
     * el modelo de paciente es bastante sencillo
     */
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(20)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [MaxLength(20)]
    public string Apellido {get; set;} = string.Empty;
    [Required]
    [MaxLength(10)]
    public string Cedula { get; set; } = string.Empty;
    [MaxLength(15)] 
    public string Telefono {get; set;} = string.Empty;
    [ MaxLength(100)]
    public string Direccion { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

}