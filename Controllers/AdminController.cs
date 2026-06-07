using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.ViewModels;

namespace SmartMedicalSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ─────────────────────────────────────────
        // GET: /Admin/Index — Dashboard
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalPatients = await _db.Patients.CountAsync(),
                TotalDoctors = await _db.Doctors.CountAsync(),
                TotalPrescriptions = await _db.Prescriptions
                    .CountAsync(),
                TotalAlerts = await _db.ValidationAlerts
                    .CountAsync(),
                HighRiskPatients = await _db.Prescriptions
                    .CountAsync(p => p.AIRiskLevel == "High"),
                LowStockMedicines = await _db.Medicines
                    .CountAsync(m =>
                        m.StockQuantity <= m.ReorderLevel),

                RecentAlerts = await _db.ValidationAlerts
                    .Include(a => a.Prescription)
                        .ThenInclude(p => p!.Patient)
                        .ThenInclude(p => p!.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync(),

                AlertsByType = await _db.ValidationAlerts
                    .GroupBy(a => a.AlertType)
                    .Select(g => new
                    {
                        g.Key,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(
                        x => x.Key,
                        x => x.Count)
            };

            return View(model);
        }

        // ─────────────────────────────────────────
        // GET: /Admin/Users
        // ─────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ToListAsync();
            return View(users);
        }

        // ─────────────────────────────────────────
        // POST: /Admin/ToggleUser/{id}
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _db.SaveChangesAsync();
                TempData["Success"] =
                    $"User {user.Name} has been " +
                    $"{(user.IsActive ? "activated" : "deactivated")}.";
            }
            return RedirectToAction("Users");
        }

        // ─────────────────────────────────────────
        // GET: /Admin/Medicines
        // ─────────────────────────────────────────
        public async Task<IActionResult> Medicines()
        {
            var medicines = await _db.Medicines
                .OrderBy(m => m.GenericName)
                .ToListAsync();
            return View(medicines);
        }

        // ─────────────────────────────────────────
        // GET: /Admin/AddMedicine
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult AddMedicine() => View();

        // ─────────────────────────────────────────
        // POST: /Admin/AddMedicine
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddMedicine(Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                _db.Medicines.Add(medicine);
                await _db.SaveChangesAsync();
                TempData["Success"] =
                    $"{medicine.GenericName} added to medicines database.";
                return RedirectToAction("Medicines");
            }
            return View(medicine);
        }

        // ─────────────────────────────────────────
        // GET: /Admin/DrugInteractions
        // ─────────────────────────────────────────
        public async Task<IActionResult> DrugInteractions()
        {
            var interactions = await _db.DrugInteractions
                .Include(d => d.Drug1)
                .Include(d => d.Drug2)
                .OrderByDescending(d => d.Severity)
                .ToListAsync();
            return View(interactions);
        }

        // ─────────────────────────────────────────
        // GET: /Admin/AuditLog
        // ─────────────────────────────────────────
        public async Task<IActionResult> AuditLog()
        {
            var logs = await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();
            return View(logs);
        }
    }
}