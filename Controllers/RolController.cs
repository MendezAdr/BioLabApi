using Microsoft.AspNetCore.Mvc;

namespace BioLabAPI.Controllers
{
    public class RolController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
