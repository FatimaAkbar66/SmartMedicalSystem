using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.ViewModels;
using SmartMedicalSystem.Services;
using System.Security.Claims;

namespace SmartMedicalSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RiskPredictionService _riskService;

        public PatientController(
            ApplicationDbContext db,
            RiskPredictionService riskService)
        {
            _db = db;
            _riskService = riskService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);

        // ─────────────────────────────────────────
        // GET: /Patient/Index — Dashboard
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var patient = await _db.Patients
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (patient == null)
                return View("NoProfile");

            var risk = _riskService.AssessRisk(
                patient,
                patient.MedicalHistories.ToList(),
                patient.Allergies.ToList(),
                patient.Prescriptions
                    .Where(p => p.Status == "Active")
                    .Sum(p => p.Items.Count));

            var model = new PatientDashboardViewModel
            {
                Patient = patient,
                RecentPrescriptions = patient.Prescriptions
                    .OrderByDescending(p => p.DateIssued)
                    .Take(5).ToList(),
                Allergies = patient.Allergies.ToList(),
                MedicalHistories = patient.MedicalHistories.ToList(),
                RiskAssessment = risk,
                TotalPrescriptions = patient.Prescriptions.Count,
                ActivePrescriptions = patient.Prescriptions
                    .Count(p => p.Status == "Active")
            };

            return View(model);
        }

        // ─────────────────────────────────────────
        // GET: /Patient/Prescriptions
        // ─────────────────────────────────────────
        public async Task<IActionResult> Prescriptions()
        {
            var userId = GetUserId();
            var patient = await _db.Patients
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (patient == null) return NotFound();

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.ValidationAlerts)
                .Where(p => p.PatientID == patient.PatientID)
                .OrderByDescending(p => p.DateIssued)
                .ToListAsync();

            return View(prescriptions);
        }

        // ─────────────────────────────────────────
        // GET: /Patient/AddAllergy
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AddAllergy()
        {
            var userId = GetUserId();
            var patient = await _db.Patients
                .FirstOrDefaultAsync(p => p.UserID == userId);
            ViewBag.PatientId = patient?.PatientID;
            return View();
        }

        // ─────────────────────────────────────────
        // POST: /Patient/AddAllergy
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddAllergy(Allergy allergy)
        {
            if (ModelState.IsValid)
            {
                _db.Allergies.Add(allergy);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Allergy added to your profile.";
                return RedirectToAction("Index");
            }
            return View(allergy);
        }

        // ─────────────────────────────────────────
        // GET: /Patient/AddHistory
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AddHistory()
        {
            var userId = GetUserId();
            var patient = await _db.Patients
                .FirstOrDefaultAsync(p => p.UserID == userId);
            ViewBag.PatientId = patient?.PatientID;
            return View();
        }

        // ─────────────────────────────────────────
        // POST: /Patient/AddHistory
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddHistory(
            MedicalHistory history)
        {
            if (ModelState.IsValid)
            {
                history.DateRecorded = DateTime.UtcNow;
                _db.MedicalHistories.Add(history);
                await _db.SaveChangesAsync();
                TempData["Success"] =
                    "Medical history updated successfully.";
                return RedirectToAction("Index");
            }
            return View(history);
        }
    }
}