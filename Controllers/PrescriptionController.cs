using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.ViewModels;
using SmartMedicalSystem.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SmartMedicalSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PrescriptionValidationService _validator;
        private readonly RiskPredictionService _riskService;
        private readonly PrescriptionPdfService _pdfService;
        private readonly AuditService _audit;

        public PrescriptionController(
            ApplicationDbContext db,
            PrescriptionValidationService validator,
            RiskPredictionService riskService,
            PrescriptionPdfService pdfService,
            AuditService audit)
        {
            _db = db;
            _validator = validator;
            _riskService = riskService;
            _pdfService = pdfService;
            _audit = audit;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(
                ClaimTypes.NameIdentifier)!.Value);

        // ─────────────────────────────────────────
        // GET: /Prescription/Create
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int? patientId)
        {
            var model = new CreatePrescriptionViewModel
            {
                AvailablePatients = await _db.Patients
                    .Include(p => p.User)
                    .OrderBy(p => p.User!.Name)
                    .ToListAsync(),

                AvailableMedicines = await _db.Medicines
                    .OrderBy(m => m.GenericName)
                    .ToListAsync()
            };

            if (patientId.HasValue)
            {
                model.PatientID = patientId.Value;
                model.SelectedPatient = model.AvailablePatients
                    .FirstOrDefault(p => p.PatientID == patientId.Value);
            }

            return View(model);
        }

        // ─────────────────────────────────────────
        // POST: /Prescription/Validate  (AJAX)
        // Called from JavaScript for live validation
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Validate(
            [FromBody] ValidateRequest req)
        {
            // Run 4-layer validation
            var result = await _validator.ValidateAsync(
                req.PatientId, req.Items);

            // Add AI risk to response
            var patient = await _db.Patients
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .FirstOrDefaultAsync(p => p.PatientID == req.PatientId);

            if (patient != null)
            {
                result.RiskAssessment = _riskService.AssessRisk(
                    patient,
                    patient.MedicalHistories.ToList(),
                    patient.Allergies.ToList(),
                    req.Items.Count);
            }

            return Json(result);
        }

        // ─────────────────────────────────────────
        // POST: /Prescription/Create
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(
    CreatePrescriptionViewModel model,
    [FromForm] string medicinesJson,
    [FromForm] string? overrideReason)
        {
            var userId = GetUserId();
            var doctor = await _db.Doctors
                .FirstOrDefaultAsync(d => d.UserID == userId);

            if (doctor == null) return Unauthorized();

            // Guard — empty json
            if (string.IsNullOrEmpty(medicinesJson))
                medicinesJson = "[]";

            // Parse medicines list from hidden field
            List<PrescriptionItemViewModel> items;
            try
            {
                items = System.Text.Json.JsonSerializer
                    .Deserialize<List<PrescriptionItemViewModel>>(
                        medicinesJson,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<PrescriptionItemViewModel>();
            }
            catch
            {
                items = new List<PrescriptionItemViewModel>();
            }

            // Run 4-layer validation
            var validation = await _validator
                .ValidateAsync(model.PatientID, items);

            // Block critical alerts unless overridden
            if (validation.HasCriticalAlerts &&
                string.IsNullOrEmpty(overrideReason))
            {
                TempData["ValidationFailed"] = validation.Message;
                model.AvailablePatients = await _db.Patients
                    .Include(p => p.User)
                    .ToListAsync();
                model.AvailableMedicines = await _db.Medicines
                    .OrderBy(m => m.GenericName)
                    .ToListAsync();
                return View(model);
            }

            // Load patient for AI risk
            var patient = await _db.Patients
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .FirstOrDefaultAsync(p => p.PatientID == model.PatientID);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Create");
            }

            var risk = _riskService.AssessRisk(
                patient,
                patient.MedicalHistories.ToList(),
                patient.Allergies.ToList(),
                items.Count);

            // Create Prescription record
            var prescription = new Prescription
            {
                PrescriptionCode = GenerateRxCode(),
                DoctorID = doctor.DoctorID,
                PatientID = model.PatientID,
                Diagnosis = model.Diagnosis,
                Notes = model.Notes,
                DateIssued = DateTime.UtcNow,
                Status = "Active",
                AIRiskLevel = risk.RiskLevel,
                AIRiskScore = risk.RiskScore,
                HasAlerts = validation.Alerts.Any()
            };

            _db.Prescriptions.Add(prescription);
            await _db.SaveChangesAsync();

            // Save prescription items
            foreach (var item in items)
            {
                _db.PrescriptionItems.Add(new PrescriptionItem
                {
                    PrescriptionID = prescription.PrescriptionID,
                    MedicineID = item.MedicineID,
                    Dosage = item.Dosage,
                    Frequency = item.Frequency,
                    DurationDays = item.DurationDays,
                    Instructions = item.Instructions,
                    Route = item.Route
                });
            }
            await _db.SaveChangesAsync();

            // Save validation alerts
            await _validator.SaveAlertsAsync(
                prescription.PrescriptionID,
                validation.Alerts);

            // Save override reason if provided
            if (!string.IsNullOrEmpty(overrideReason))
            {
                var savedAlerts = await _db.ValidationAlerts
                    .Where(a => a.PrescriptionID ==
                                prescription.PrescriptionID)
                    .ToListAsync();

                foreach (var alert in savedAlerts)
                {
                    alert.IsOverridden = true;
                    alert.OverrideReason = overrideReason;
                    alert.OverriddenBy = User.Identity?.Name;
                }
                await _db.SaveChangesAsync();
            }

            // Audit log
            await _audit.LogAsync(
                userId, "CreatePrescription", "Prescription",
                $"Rx {prescription.PrescriptionCode} for " +
                $"PatientID {model.PatientID}");

            TempData["Success"] =
                $"Prescription {prescription.PrescriptionCode} " +
                $"created successfully!";

            return RedirectToAction("Detail",
                new { id = prescription.PrescriptionID });
        }

        // ─────────────────────────────────────────
        // GET: /Prescription/Detail/{id}
        // ─────────────────────────────────────────
        public async Task<IActionResult> Detail(int id)
        {
            var prescription = await _db.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.Allergies)
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.MedicalHistories)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.ValidationAlerts)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound();

            var patient = prescription.Patient!;

            var risk = _riskService.AssessRisk(
                patient,
                patient.MedicalHistories.ToList(),
                patient.Allergies.ToList(),
                prescription.Items.Count);

            var model = new PrescriptionDetailViewModel
            {
                Prescription = prescription,
                Alerts = prescription.ValidationAlerts.ToList(),
                RiskAssessment = risk,
                CanDispense = prescription.Status == "Active"
            };

            return View(model);
        }

        // ─────────────────────────────────────────
        // GET: /Prescription/List
        // ─────────────────────────────────────────
        public async Task<IActionResult> List()
        {
            var userId = GetUserId();
            var doctor = await _db.Doctors
                .FirstOrDefaultAsync(d => d.UserID == userId);

            if (doctor == null)
                return NotFound();

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.User)
                .Include(p => p.ValidationAlerts)
                .Where(p => p.DoctorID == doctor.DoctorID)
                .OrderByDescending(p => p.DateIssued)
                .ToListAsync();

            return View(prescriptions);
        }

        // ─────────────────────────────────────────
        // GET: /Prescription/DownloadPdf/{id}
        // ─────────────────────────────────────────
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var prescription = await _db.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.User)
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.Allergies)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.ValidationAlerts)
                .FirstOrDefaultAsync(p =>
                    p.PrescriptionID == id);

            if (prescription == null)
                return NotFound();

            var bytes = _pdfService.GeneratePdf(prescription);

            // Serve as HTML — browser can print to PDF
            return File(bytes, "text/html",
                $"Prescription_{prescription.PrescriptionCode}.html");
        }

        // ─────────────────────────────────────────
        // HELPER — generate unique Rx code
        // ─────────────────────────────────────────
        private static string GenerateRxCode()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"RX-{date}-{random}";
        }
    }
}