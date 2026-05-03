using Microsoft.AspNetCore.Mvc;

namespace BioLabAPI.Controllers
{
    public class ExamenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
