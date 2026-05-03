using Microsoft.AspNetCore.Mvc;

namespace BioLabAPI.Controllers
{
    public class PagosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
