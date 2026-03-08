using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
