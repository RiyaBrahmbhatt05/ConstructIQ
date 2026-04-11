using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class PermitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Permits
        public IActionResult Index()
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var permits = _context.Permits
                .OrderByDescending(p => p.ApplicationDate)
                .ToList();

            return View(permits);
        }

        // GET: Permits/Details/5
        public IActionResult Details(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var permit = _context.Permits.FirstOrDefault(p => p.PermitId == id);
            if (permit == null)
            {
                return NotFound();
            }

            ViewBag.LinkedTasksCount = _context.Tasks.Count(t => t.PermitId == id);
            return View(permit);
        }

        // GET: Permits/Create
        public IActionResult Create()
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            return View(new Permit
            {
                Status = "Pending",
                ApplicationDate = DateTime.Today
            });
        }

        // POST: Permits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Permit permit)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            ValidatePermitDates(permit);

            if (!ModelState.IsValid)
            {
                return View(permit);
            }

            _context.Permits.Add(permit);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Permit '{permit.Type}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Permits/Edit/5
        public IActionResult Edit(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var permit = _context.Permits.FirstOrDefault(p => p.PermitId == id);
            if (permit == null)
            {
                return NotFound();
            }

            return View(permit);
        }

        // POST: Permits/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Permit permit)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            if (id != permit.PermitId)
            {
                return NotFound();
            }

            ValidatePermitDates(permit);

            if (!ModelState.IsValid)
            {
                return View(permit);
            }

            var existingPermit = _context.Permits.FirstOrDefault(p => p.PermitId == id);
            if (existingPermit == null)
            {
                return NotFound();
            }

            existingPermit.Type = permit.Type;
            existingPermit.Status = permit.Status;
            existingPermit.ApplicationDate = permit.ApplicationDate;
            existingPermit.ApprovalDate = permit.ApprovalDate;
            existingPermit.ExpiryDate = permit.ExpiryDate;
            existingPermit.Fee = permit.Fee;
            existingPermit.IssuingAuthority = permit.IssuingAuthority;
            existingPermit.Notes = permit.Notes;

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Permit '{existingPermit.Type}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Permits/Delete/5
        public IActionResult Delete(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var permit = _context.Permits.FirstOrDefault(p => p.PermitId == id);
            if (permit == null)
            {
                return NotFound();
            }

            ViewBag.LinkedTasksCount = _context.Tasks.Count(t => t.PermitId == id);
            return View(permit);
        }

        // POST: Permits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var authRedirect = RedirectIfNotLoggedIn();
            if (authRedirect != null)
            {
                return authRedirect;
            }

            var permit = _context.Permits.FirstOrDefault(p => p.PermitId == id);
            if (permit == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var linkedTasksCount = _context.Tasks.Count(t => t.PermitId == id);
            if (linkedTasksCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete permit '{permit.Type}' because it is assigned to {linkedTasksCount} task(s).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Permits.Remove(permit);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Permit '{permit.Type}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidatePermitDates(Permit permit)
        {
            if (permit.ApprovalDate.HasValue && permit.ApprovalDate.Value < permit.ApplicationDate)
            {
                ModelState.AddModelError(nameof(permit.ApprovalDate), "Approval date cannot be before application date.");
            }

            if (permit.ExpiryDate.HasValue && permit.ApprovalDate.HasValue && permit.ExpiryDate.Value < permit.ApprovalDate.Value)
            {
                ModelState.AddModelError(nameof(permit.ExpiryDate), "Expiry date cannot be before approval date.");
            }

            if (permit.ExpiryDate.HasValue && !permit.ApprovalDate.HasValue && permit.ExpiryDate.Value < permit.ApplicationDate)
            {
                ModelState.AddModelError(nameof(permit.ExpiryDate), "Expiry date cannot be before application date.");
            }
        }

        private IActionResult? RedirectIfNotLoggedIn()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }
    }
}
