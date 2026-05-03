using Microsoft.AspNetCore.Mvc;

namespace BioLabAPI.Controllers
{
    public class PacienteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
