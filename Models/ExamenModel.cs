using System;

namespace BioLabApi.Models;

public class ExamenModel : Auditable
{
    //simplemente es el modelo basico para un examen

    public int Id { get; set; }
    public string NombreExamen { get; set; } = string.Empty;
    public decimal CostoEnDivisa { get; set; }
    public decimal CostoenBolivares {get;set;}
    public string Descripcion { get; set; } = string.Empty;
}
