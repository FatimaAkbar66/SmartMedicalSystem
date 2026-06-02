using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.ViewModels;

namespace SmartMedicalSystem.Services
{
    public class PrescriptionValidationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PrescriptionValidationService> _logger;

        public PrescriptionValidationService(
            ApplicationDbContext db,
            ILogger<PrescriptionValidationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────
        // MAIN — runs all 4 validation checks
        // ─────────────────────────────────────────────────────
        public async Task<ValidationResultViewModel> ValidateAsync(
            int patientId,
            List<PrescriptionItemViewModel> items)
        {
            var result = new ValidationResultViewModel { IsValid = true };

            // Guard: need at least 1 medicine
            if (!items.Any())
            {
                result.IsValid = false;
                result.Message = "Prescription must include at least one medication.";
                return result;
            }

            // Load patient with allergies + history
            var patient = await _db.Patients
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .FirstOrDefaultAsync(p => p.PatientID == patientId);

            if (patient == null)
            {
                result.IsValid = false;
                result.Message = "Patient not found.";
                return result;
            }

            // Load selected medicines
            var medIds = items.Select(i => i.MedicineID).ToList();
            var medicines = await _db.Medicines
                .Where(m => medIds.Contains(m.MedicineID))
                .ToListAsync();

            var alerts = new List<AlertItemViewModel>();

            // ── CHECK 1: Allergies ─────────────────────────
            alerts.AddRange(
                await CheckAllergiesAsync(patient, medicines));

            // ── CHECK 2: Drug Interactions ─────────────────
            alerts.AddRange(
                await CheckInteractionsAsync(medicines));

            // ── CHECK 3: Dosage Validation ─────────────────
            alerts.AddRange(
                CheckDosage(patient, items, medicines));

            // ── CHECK 4: Duplicate Medications ────────────
            alerts.AddRange(
                CheckDuplicates(medicines));

            // Build final result
            result.Alerts = alerts;
            result.HasCriticalAlerts = alerts.Any(a => a.Severity == "Critical");
            result.IsValid = !result.HasCriticalAlerts;

            result.Message = result.IsValid
                ? (alerts.Any()
                    ? $"⚠ Prescription has {alerts.Count} warning(s). Review before finalizing."
                    : "✅ All safety checks passed. Safe to finalize.")
                : $"🚫 Blocked: {alerts.Count(a => a.Severity == "Critical")} critical issue(s) detected.";

            _logger.LogInformation(
                "Validation — Patient {id}: Valid={v}, Alerts={n}",
                patientId, result.IsValid, alerts.Count);

            return result;
        }

        // ─────────────────────────────────────────────────────
        // CHECK 1 — ALLERGY DETECTION
        // ─────────────────────────────────────────────────────
        private async Task<List<AlertItemViewModel>> CheckAllergiesAsync(
            Patient patient, List<Medicine> medicines)
        {
            var alerts = new List<AlertItemViewModel>();
            var allergens = patient.Allergies
                .Select(a => a.Allergen.ToLower().Trim())
                .ToList();

            foreach (var med in medicines)
            {
                var medNameLower = med.GenericName.ToLower();
                var ingredientLower = med.ActiveIngredient.ToLower();
                var brandsLower = med.BrandNames.ToLower();

                // Direct name match
                var match = allergens.FirstOrDefault(a =>
                    medNameLower.Contains(a) ||
                    ingredientLower.Contains(a) ||
                    brandsLower.Contains(a) ||
                    a.Contains(medNameLower));

                if (match != null)
                {
                    var allergyRecord = patient.Allergies
                        .First(a => a.Allergen.ToLower().Trim() == match);

                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Allergy",
                        Severity = allergyRecord.Severity == "Severe"
                                   ? "Critical" : "Warning",
                        Message =
                            $"ALLERGY CONFLICT: {med.GenericName} conflicts " +
                            $"with documented {allergyRecord.Allergen} allergy " +
                            $"(Severity: {allergyRecord.Severity}). " +
                            $"Known reaction: {allergyRecord.Reaction ?? "Not specified"}",
                        SuggestedAction =
                            $"Remove {med.GenericName} and choose an " +
                            $"alternative without {allergyRecord.Allergen}"
                    });
                }

                // Cross-reactivity: Penicillin → Amoxicillin/Ampicillin
                if (allergens.Contains("penicillin") &&
                    med.Category.ToLower().Contains("penicillin"))
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Allergy",
                        Severity = "Critical",
                        Message =
                            $"CROSS-REACTIVITY: {med.GenericName} belongs to the " +
                            $"Penicillin class. Patient has documented Penicillin allergy.",
                        SuggestedAction =
                            "Consider Azithromycin or Clindamycin as alternatives"
                    });
                }

                // Contraindication allergy check
                if (!string.IsNullOrEmpty(med.Contraindications))
                {
                    foreach (var allergen in allergens)
                    {
                        if (med.Contraindications.ToLower()
                                .Contains(allergen + " allergy") &&
                            !alerts.Any(a =>
                                a.Type == "Allergy" &&
                                a.Message.Contains(med.GenericName)))
                        {
                            alerts.Add(new AlertItemViewModel
                            {
                                Type = "Allergy",
                                Severity = "Critical",
                                Message =
                                    $"CONTRAINDICATION: {med.GenericName} is " +
                                    $"contraindicated in patients with {allergen} allergy.",
                                SuggestedAction =
                                    "Review contraindications and select an alternative"
                            });
                        }
                    }
                }
            }

            return alerts;
        }

        // ─────────────────────────────────────────────────────
        // CHECK 2 — DRUG INTERACTION DETECTION
        // ─────────────────────────────────────────────────────
        private async Task<List<AlertItemViewModel>> CheckInteractionsAsync(
            List<Medicine> medicines)
        {
            var alerts = new List<AlertItemViewModel>();
            var ids = medicines.Select(m => m.MedicineID).ToList();

            var interactions = await _db.DrugInteractions
                .Include(di => di.Drug1)
                .Include(di => di.Drug2)
                .Where(di =>
                    ids.Contains(di.Drug1ID) &&
                    ids.Contains(di.Drug2ID))
                .ToListAsync();

            foreach (var interaction in interactions)
            {
                alerts.Add(new AlertItemViewModel
                {
                    Type = "DrugInteraction",
                    Severity = interaction.Severity == "Severe"
                               ? "Critical"
                               : interaction.Severity == "Moderate"
                               ? "Warning" : "Info",
                    Message =
                        $"DRUG INTERACTION ({interaction.Severity.ToUpper()}): " +
                        $"{interaction.Drug1?.GenericName} + " +
                        $"{interaction.Drug2?.GenericName} — " +
                        $"{interaction.Description}. " +
                        $"Effect: {interaction.ClinicalEffect}",
                    SuggestedAction = interaction.ManagementAdvice
                });
            }

            return alerts;
        }

        // ─────────────────────────────────────────────────────
        // CHECK 3 — DOSAGE VALIDATION
        // ─────────────────────────────────────────────────────
        private List<AlertItemViewModel> CheckDosage(
            Patient patient,
            List<PrescriptionItemViewModel> items,
            List<Medicine> medicines)
        {
            var alerts = new List<AlertItemViewModel>();

            double age = patient.DateOfBirth.HasValue
                ? (DateTime.Today - patient.DateOfBirth.Value).TotalDays / 365.25
                : 30;
            double weight = patient.Weight > 0 ? patient.Weight : 70;

            foreach (var item in items)
            {
                var med = medicines
                    .FirstOrDefault(m => m.MedicineID == item.MedicineID);
                if (med == null) continue;

                // Overdose check
                if (item.Dosage > med.MaxDosageMg)
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Dosage",
                        Severity = "Critical",
                        Message =
                            $"OVERDOSE RISK: {med.GenericName} — prescribed " +
                            $"{item.Dosage}mg exceeds maximum safe dose " +
                            $"of {med.MaxDosageMg}mg.",
                        SuggestedAction =
                            $"Reduce dose to ≤{med.MaxDosageMg}mg"
                    });
                }

                // Underdose check
                if (item.Dosage < med.MinDosageMg && item.Dosage > 0)
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Dosage",
                        Severity = "Warning",
                        Message =
                            $"UNDERDOSE: {med.GenericName} — {item.Dosage}mg " +
                            $"is below minimum therapeutic dose of {med.MinDosageMg}mg.",
                        SuggestedAction =
                            $"Increase dose to at least {med.MinDosageMg}mg"
                    });
                }

                // Pediatric safety
                if (med.AgeGroup == "Adult" && age < 18)
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Dosage",
                        Severity = "Critical",
                        Message =
                            $"PEDIATRIC SAFETY: {med.GenericName} is approved " +
                            $"for adults only. Patient age: {age:F0} years.",
                        SuggestedAction =
                            "Consult pediatric dosing guidelines or select a safer alternative"
                    });
                }

                // Geriatric high-dose warning
                if (age >= 65 && item.Dosage > med.MaxDosageMg * 0.75)
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Dosage",
                        Severity = "Warning",
                        Message =
                            $"GERIATRIC DOSING: {med.GenericName} dose may be " +
                            $"high for elderly patient. Apply 'Start Low, Go Slow'.",
                        SuggestedAction =
                            $"Consider starting at " +
                            $"{med.MaxDosageMg * 0.5:F0}–{med.MaxDosageMg * 0.75:F0}mg"
                    });
                }

                // Antibiotic duration warning
                if (item.DurationDays > 14 &&
                    med.Category.ToLower().Contains("antibiotic"))
                {
                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Dosage",
                        Severity = "Warning",
                        Message =
                            $"ANTIBIOTIC STEWARDSHIP: {med.GenericName} " +
                            $"prescribed for {item.DurationDays} days. " +
                            $"Prolonged antibiotic use promotes resistance.",
                        SuggestedAction =
                            "Standard antibiotic course is 5–14 days. Verify necessity."
                    });
                }
            }

            return alerts;
        }

        // ─────────────────────────────────────────────────────
        // CHECK 4 — DUPLICATE MEDICATION DETECTION
        // ─────────────────────────────────────────────────────
        private List<AlertItemViewModel> CheckDuplicates(
            List<Medicine> medicines)
        {
            var alerts = new List<AlertItemViewModel>();

            // Same active ingredient
            var ingredientGroups = medicines
                .GroupBy(m => m.ActiveIngredient.ToLower().Trim())
                .Where(g => g.Count() > 1);

            foreach (var group in ingredientGroups)
            {
                var names = string.Join(" and ",
                    group.Select(m => m.GenericName));

                alerts.Add(new AlertItemViewModel
                {
                    Type = "Duplicate",
                    Severity = "Critical",
                    Message =
                        $"DUPLICATE MEDICATION: {names} share the same " +
                        $"active ingredient ({group.Key.ToUpper()}). " +
                        $"Combined use creates overdose risk.",
                    SuggestedAction =
                        $"Remove one preparation. Keep only one form of {group.Key}."
                });
            }

            // Same drug class duplicates
            var dangerousClasses = new[]
            {
                "nsaid", "anticoagulant", "antibiotic", "statin", "antiplatelet"
            };

            foreach (var drugClass in dangerousClasses)
            {
                var classGroup = medicines
                    .Where(m => m.Category.ToLower().Contains(drugClass))
                    .ToList();

                if (classGroup.Count > 1 &&
                    !alerts.Any(a =>
                        a.Type == "Duplicate" &&
                        a.Message.Contains(drugClass)))
                {
                    var names = string.Join(" and ",
                        classGroup.Select(m => m.GenericName));

                    alerts.Add(new AlertItemViewModel
                    {
                        Type = "Duplicate",
                        Severity = "Warning",
                        Message =
                            $"SAME CLASS: {names} both belong to " +
                            $"{drugClass.ToUpper()} class. " +
                            $"Concurrent use requires clinical justification.",
                        SuggestedAction =
                            "Verify clinical necessity of two agents from the same class"
                    });
                }
            }

            return alerts;
        }

        // ─────────────────────────────────────────────────────
        // SAVE alerts to database after prescription is created
        // ─────────────────────────────────────────────────────
        public async Task SaveAlertsAsync(
            int prescriptionId,
            List<AlertItemViewModel> alertItems)
        {
            foreach (var item in alertItems)
            {
                _db.ValidationAlerts.Add(new ValidationAlert
                {
                    PrescriptionID = prescriptionId,
                    AlertType = item.Type,
                    Severity = item.Severity,
                    Message = item.Message,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}