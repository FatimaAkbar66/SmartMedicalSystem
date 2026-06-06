using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.ViewModels;
using SmartMedicalSystem.Services;
using System.Security.Claims;

namespace SmartMedicalSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AuditService _audit;

        public AccountController(
            ApplicationDbContext db,
            AuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        // ─────────────────────────────────────────
        // GET: /Account/Login
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            // Already logged in → redirect to dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        // ─────────────────────────────────────────
        // POST: /Account/Login
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find active user by email
            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == model.Email && u.IsActive);

            // Verify password
            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("",
                    "Invalid email or password. Please try again.");
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name,            user.Name),
                new Claim(ClaimTypes.Email,           user.Email),
                new Claim(ClaimTypes.Role,            user.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal, props);

            // Audit log
            await _audit.LogAsync(
                user.UserID, "Login", "User",
                $"Login successful. Role: {user.Role}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return RedirectToDashboard();
        }

        // ─────────────────────────────────────────
        // GET: /Account/Register
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        // ─────────────────────────────────────────
        // POST: /Account/Register
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check duplicate email
            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email",
                    "This email is already registered.");
                return View(model);
            }

            // Create user
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Create role-specific profile
            if (model.Role == "Doctor")
            {
                _db.Doctors.Add(new Doctor
                {
                    UserID = user.UserID,
                    Specialization = model.Specialization ?? "General",
                    LicenseNo = model.LicenseNo ?? "PENDING",
                    Department = model.Department ?? "General Medicine",
                    Phone = model.Phone
                });
            }
            else if (model.Role == "Patient")
            {
                _db.Patients.Add(new Patient
                {
                    UserID = user.UserID,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender ?? "Not Specified",
                    BloodGroup = model.BloodGroup ?? "Unknown",
                    Weight = model.Weight ?? 0,
                    Height = model.Height ?? 0,
                    Phone = model.Phone
                });
            }

            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                user.UserID, "Register", "User",
                $"New {model.Role} registered: {model.Email}");

            TempData["Success"] =
                "Registration successful! Please login.";
            return RedirectToAction("Login");
        }

        // ─────────────────────────────────────────
        // GET: /Account/Logout
        // ─────────────────────────────────────────
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ─────────────────────────────────────────
        // GET: /Account/AccessDenied
        // ─────────────────────────────────────────
        public IActionResult AccessDenied() => View();

        // ─────────────────────────────────────────
        // HELPER — redirect based on role
        // ─────────────────────────────────────────
        private IActionResult RedirectToDashboard()
        {
            return User.IsInRole("Admin") ? RedirectToAction("Index", "Admin")
                 : User.IsInRole("Doctor") ? RedirectToAction("Index", "Doctor")
                 : User.IsInRole("Pharmacist") ? RedirectToAction("Index", "Pharmacy")
                 : RedirectToAction("Index", "Patient");
        }
    }
}