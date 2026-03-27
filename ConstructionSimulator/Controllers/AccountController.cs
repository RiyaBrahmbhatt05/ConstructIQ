using Microsoft.AspNetCore.Mvc;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;

namespace ConstructionSimulator.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authService;

        public AccountController(AuthenticationService authService)
        {
            _authService = authService;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // If user is already logged in, redirect to home
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, user) = await _authService.LoginAsync(model.Email, model.Password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }

            // Set session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);

            // If remember me is checked, set cookie for longer period
            if (model.RememberMe)
            {
                Response.Cookies.Append("UserEmail", user.Email,
                    new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
            }

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
            return RedirectToLocal(returnUrl);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // If user is already logged in, redirect to home
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message) = await _authService.RegisterAsync(model.FullName, model.Email, model.Password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
            return RedirectToAction("Login");
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new ForgotPasswordViewModel());
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["SuccessMessage"] = "If an account exists for that email, a password reset link has been sent.";
            return RedirectToAction("Login");
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserName");

            // Clear cookie if it exists
            Response.Cookies.Delete("UserEmail");

            TempData["InfoMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Landing");
        }

        // Helper method
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}