using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstructionSimulator.Data;
using ConstructionSimulator.Services;
using ConstructionSimulator.ViewModels;
using ConstructionSimulator.Models;

namespace ConstructionSimulator.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authService;
        private readonly ApplicationDbContext _dbContext;

        public AccountController(AuthenticationService authService, ApplicationDbContext dbContext)
        {
            _authService = authService;
            _dbContext = dbContext;
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

        // GET: Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return View(model);
        }

        // GET: Account/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email
            };

            return View(model);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var emailInUse = await _dbContext.Users
                .AnyAsync(u => u.Id != user.Id && u.Email == normalizedEmail);

            if (emailInUse)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already in use by another account.");
                return View(model);
            }

            user.FullName = model.FullName.Trim();
            user.Email = normalizedEmail;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // GET: Account/DeleteProfile
        [HttpGet]
        public async Task<IActionResult> DeleteProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new DeleteProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email
            };

            return View(model);
        }

        // POST: Account/DeleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile(DeleteProfileViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            model.FullName = user.FullName;
            model.Email = user.Email;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(nameof(model.Password), "Incorrect password.");
                return View(model);
            }

            user.IsActive = false;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserName");
            Response.Cookies.Delete("UserEmail");

            TempData["InfoMessage"] = "Your account has been deleted.";
            return RedirectToAction("Index", "Landing");
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

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userIdValue = HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdValue, out var userId))
            {
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var normalizedEmail = userEmail.Trim().ToLowerInvariant();
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsActive);
            }

            return null;
        }
    }
}