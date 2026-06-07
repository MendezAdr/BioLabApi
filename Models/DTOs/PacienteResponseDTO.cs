namespace BioLabApi.Models.DTOs
{
    public class PacienteResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = "N/A";

        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = "N/A";
        public string Direccion { get; set; } = "N/A";

        public string NombreAcompañante { get; set; } = "N/A";
        public string CedulaAcompañante { get; set; } = "N/A";


    }
}
