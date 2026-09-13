using Microsoft.AspNetCore.Mvc;
using BioLabApi.Helpers;

namespace BioLabApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DolarController : ControllerBase
    {
        private readonly GetDollarPrice _dolarHelper;

        // Inyectamos el Singleton que ya tienes configurado
        public DolarController(GetDollarPrice dolarHelper)
        {
            _dolarHelper = dolarHelper;
        }

        [HttpGet]
        public IActionResult GetPrecioActual()
        {
            // Verificamos si el Worker logró obtener la tasa exitosamente
            if (_dolarHelper.IsSuccess && _dolarHelper.CurrentRate != null)
            {
                return Ok(new 
                {
                    success = true,
                    tasa = _dolarHelper.CurrentRate.Promedio,
                    fecha = _dolarHelper.CurrentRate.FechaActualizacion,
                    message = "Tasa obtenida desde BCV/DolarAPI."
                });
            }

            // Si falló (no hay internet, API caída), le avisamos al frontend pacíficamente
            return Ok(new 
            {
                success = false,
                tasa = 0,
                message = string.IsNullOrEmpty(_dolarHelper.ErrorMessage) 
                            ? "Requiere ingreso manual." 
                            : _dolarHelper.ErrorMessage
            });
        }
    }
}