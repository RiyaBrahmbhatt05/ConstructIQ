using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class CrewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CrewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Crews
        public IActionResult Index()
        {
            var crews = _context.Crews.ToList();
            return View(crews);
        }

        // GET: Crews/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Crews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Crew crew)
        {
            if (ModelState.IsValid)
            {
                _context.Crews.Add(crew);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Crew '{crew.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(crew);
        }

        // GET: Crews/Edit/5
        public IActionResult Edit(int id)
        {
            var crew = _context.Crews.FirstOrDefault(c => c.CrewId == id);
            if (crew == null)
            {
                return NotFound();
            }

            return View(crew);
        }

        // POST: Crews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Crew crew)
        {
            if (id != crew.CrewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCrew = _context.Crews.FirstOrDefault(c => c.CrewId == id);
                if (existingCrew == null)
                {
                    return NotFound();
                }

                existingCrew.Name = crew.Name;
                existingCrew.SkillType = crew.SkillType;
                existingCrew.TeamSize = crew.TeamSize;
                existingCrew.HourlyRate = crew.HourlyRate;
                existingCrew.IsAvailable = crew.IsAvailable;
                existingCrew.RequiredLicenses = crew.RequiredLicenses;
                existingCrew.MemberNames = crew.MemberNames;
                existingCrew.ContactDetails = crew.ContactDetails;
                existingCrew.YearsOfExperience = crew.YearsOfExperience;
                existingCrew.Notes = crew.Notes;

                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Crew '{crew.Name}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(crew);
        }

        // GET: Crews/Delete/5
        public IActionResult Delete(int id)
        {
            var crew = _context.Crews.FirstOrDefault(c => c.CrewId == id);
            if (crew == null)
            {
                return NotFound();
            }

            return View(crew);
        }

        // POST: Crews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var crew = _context.Crews.FirstOrDefault(c => c.CrewId == id);
            if (crew != null)
            {
                _context.Crews.Remove(crew);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Crew '{crew.Name}' deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}