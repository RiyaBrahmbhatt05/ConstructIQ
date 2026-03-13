using ConstructionSimulator.Data;
using ConstructionSimulator.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace ConstructionSimulator.Services
{
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _context;

        public AuthenticationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email.ToLower());

                if (existingUser != null)
                {
                    return (false, "Email already registered");
                }

                // Hash password
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Create new user
                var newUser = new ApplicationUser
                {
                    FullName = fullName,
                    Email = email.ToLower(),
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return (true, "Registration successful!");
            }
            catch (Exception ex)
            {
                return (false, $"Error during registration: {ex.Message}");
            }
        }

        /// <summary>
        /// Authenticate user login
        /// </summary>
        public async Task<(bool Success, ApplicationUser User)> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email.ToLower() && u.IsActive);

                if (user == null)
                {
                    return (false, null);
                }

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return (false, null);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return (true, user);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower());
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<ApplicationUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}