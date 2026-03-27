using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Controllers
{
    public class ContactController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["SuccessMessage"] = "Your message has been sent successfully. Our team will contact you shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
