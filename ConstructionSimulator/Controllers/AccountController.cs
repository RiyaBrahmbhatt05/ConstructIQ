using Microsoft.AspNetCore.Mvc;

namespace ConstructionSimulator.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Login (placeholder - no logic yet)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // TODO: Add authentication logic later
            TempData["InfoMessage"] = "Login functionality will be implemented in Sprint 2";
            return RedirectToAction("Index", "Home");
        }

        // POST: Account/Register (placeholder - no logic yet)
        [HttpPost]
        public IActionResult Register(string name, string email, string password)
        {
            // TODO: Add registration logic later
            TempData["SuccessMessage"] = "Registration successful! (Demo mode - no actual account created)";
            return RedirectToAction("Login");
        }

        // Logout (placeholder)
        public IActionResult Logout()
        {
            // TODO: Add logout logic later
            return RedirectToAction("Index", "Landing");
        }
    }
}