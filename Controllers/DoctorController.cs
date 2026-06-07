using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.ViewModels;
using SmartMedicalSystem.Services;
using System.Security.Claims;

namespace SmartMedicalSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RiskPredictionService _riskService;
        private readonly AuditService _audit;

        public DoctorController(
            ApplicationDbContext db,
            RiskPredictionService riskService,
            AuditService audit)
        {
            _db = db;
            _riskService = riskService;
            _audit = audit;
        }

        // helper — get current logged-in user ID
        private int GetUserId() =>
            int.Parse(User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);

        // ─────────────────────────────────────────
        // GET: /Doctor/Index — Dashboard
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var doctor = await _db.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserID == userId);

            if (doctor == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            // All prescriptions by this doctor
            var allRx = await _db.Prescriptions
                .Include(p => p.Patient).ThenInclude(p => p!.User)
                .Include(p => p.ValidationAlerts)
                .Where(p => p.DoctorID == doctor.DoctorID)
                .OrderByDescending(p => p.DateIssued)
                .ToListAsync();

            // Recent alerts
            var recentAlerts = await _db.ValidationAlerts
                .Include(a => a.Prescription)
                    .ThenInclude(p => p!.Patient)
                    .ThenInclude(p => p!.User)
                .Where(a => a.Prescription!.DoctorID == doctor.DoctorID)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .ToListAsync();

            var model = new DoctorDashboardViewModel
            {
                Doctor = doctor,
                TotalPatients = await _db.Patients.CountAsync(),
                PrescriptionsToday = allRx.Count(p =>
                    p.DateIssued.Date == today),
                TotalAlerts = allRx.Sum(p =>
                    p.ValidationAlerts.Count),
                HighRiskCount = allRx.Count(p =>
                    p.AIRiskLevel == "High"),
                RecentPrescriptions = allRx.Take(10).ToList(),
                RecentAlerts = recentAlerts
            };

            return View(model);
        }

        // ─────────────────────────────────────────
        // GET: /Doctor/Patients
        // ─────────────────────────────────────────
        public async Task<IActionResult> Patients(string? search)
        {
            var query = _db.Patients
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p =>
                    p.User!.Name.Contains(search) ||
                    p.User.Email.Contains(search) ||
                    p.BloodGroup.Contains(search));

            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        // ─────────────────────────────────────────
        // GET: /Doctor/PatientProfile/{id}
        // ─────────────────────────────────────────
        public async Task<IActionResult> PatientProfile(int id)
        {
            var patient = await _db.Patients
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Items)
                    .ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
                return NotFound();

            // AI risk assessment
            var risk = _riskService.AssessRisk(
                patient,
                patient.MedicalHistories.ToList(),
                patient.Allergies.ToList(),
                patient.Prescriptions
                    .Where(p => p.Status == "Active")
                    .Sum(p => p.Items.Count));

            var model = new PatientProfileViewModel
            {
                Patient = patient,
                Allergies = patient.Allergies.ToList(),
                MedicalHistories = patient.MedicalHistories.ToList(),
                Prescriptions = patient.Prescriptions
                    .OrderByDescending(p => p.DateIssued)
                    .ToList(),
                RiskAssessment = risk
            };

            return View(model);
        }
    }
}