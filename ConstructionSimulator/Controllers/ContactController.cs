using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using ConstructionSimulator.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ContactController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var submission = new ContactSubmission
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Subject = model.Subject,
                SubmittedAtUtc = DateTime.UtcNow
            };

            _dbContext.ContactSubmissions.Add(submission);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your contact details were saved successfully. Our team will contact you shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
