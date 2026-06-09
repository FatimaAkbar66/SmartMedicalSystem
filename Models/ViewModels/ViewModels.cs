using System.ComponentModel.DataAnnotations;
using SmartMedicalSystem.Models.Entities;
using SmartMedicalSystem.Models.MLModels;

namespace SmartMedicalSystem.Models.ViewModels
{
    // ─────────────────────────────────────────
    // AUTH
    // ─────────────────────────────────────────
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Patient";

        // Doctor fields
        public string? Specialization { get; set; }
        public string? LicenseNo { get; set; }
        public string? Department { get; set; }

        // Patient fields
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? Phone { get; set; }
    }

    // ─────────────────────────────────────────
    // DASHBOARDS
    // ─────────────────────────────────────────
    public class DoctorDashboardViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public int TotalPatients { get; set; }
        public int PrescriptionsToday { get; set; }
        public int TotalAlerts { get; set; }
        public int HighRiskCount { get; set; }
        public List<Prescription> RecentPrescriptions { get; set; } = new();
        public List<ValidationAlert> RecentAlerts { get; set; } = new();
    }

    public class PatientDashboardViewModel
    {
        public Patient Patient { get; set; } = null!;
        public List<Prescription> RecentPrescriptions { get; set; } = new();
        public List<Allergy> Allergies { get; set; } = new();
        public List<MedicalHistory> MedicalHistories { get; set; } = new();
        public RiskAssessmentResult? RiskAssessment { get; set; }
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TotalAlerts { get; set; }
        public int HighRiskPatients { get; set; }
        public int LowStockMedicines { get; set; }
        public List<ValidationAlert> RecentAlerts { get; set; } = new();
        public Dictionary<string, int> AlertsByType { get; set; } = new();
        public Dictionary<string, int> PrescriptionsByMonth { get; set; } = new();
    }

    public class PharmacyDashboardViewModel
    {
        public List<Prescription> PendingPrescriptions { get; set; } = new();
        public List<Medicine> LowStockMedicines { get; set; } = new();
        public List<Medicine> ExpiringMedicines { get; set; } = new();
        public int DispensedToday { get; set; }
    }

    // ─────────────────────────────────────────
    // PRESCRIPTION
    // ─────────────────────────────────────────
    public class CreatePrescriptionViewModel
    {
        [Required]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Diagnosis is required")]
        public string Diagnosis { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public Patient? SelectedPatient { get; set; }
        public List<Patient> AvailablePatients { get; set; } = new();
        public List<Medicine> AvailableMedicines { get; set; } = new();
        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }

    public class PrescriptionItemViewModel
    {
        public int MedicineID { get; set; }
        public string? MedicineName { get; set; }
        public double Dosage { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public string Route { get; set; } = "Oral";
    }

    public class PrescriptionDetailViewModel
    {
        public Prescription Prescription { get; set; } = null!;
        public List<ValidationAlert> Alerts { get; set; } = new();
        public RiskAssessmentResult? RiskAssessment { get; set; }
        public bool CanDispense { get; set; }
    }

    // ─────────────────────────────────────────
    // VALIDATION RESULT (returned as JSON)
    // ─────────────────────────────────────────
    public class ValidationResultViewModel
    {
        public bool IsValid { get; set; }
        public bool HasCriticalAlerts { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AlertItemViewModel> Alerts { get; set; } = new();
        public RiskAssessmentResult? RiskAssessment { get; set; }
    }

    public class AlertItemViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? SuggestedAction { get; set; }

        // Computed for UI
        public string CssClass => Severity switch
        {
            "Critical" => "danger",
            "Warning" => "warning",
            _ => "info"
        };
        public string Icon => Type switch
        {
            "Allergy" => "bi-exclamation-triangle-fill",
            "DrugInteraction" => "bi-capsule",
            "Dosage" => "bi-thermometer-half",
            "Duplicate" => "bi-files",
            _ => "bi-info-circle"
        };
    }

    // ─────────────────────────────────────────
    // PATIENT PROFILE
    // ─────────────────────────────────────────
    public class PatientProfileViewModel
    {
        public Patient Patient { get; set; } = null!;
        public List<Allergy> Allergies { get; set; } = new();
        public List<MedicalHistory> MedicalHistories { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
        public RiskAssessmentResult? RiskAssessment { get; set; }
    }

    // ─────────────────────────────────────────
    // VALIDATE REQUEST (from JS fetch call)
    // ─────────────────────────────────────────
    public class ValidateRequest
    {
        public int PatientId { get; set; }
        public List<PrescriptionItemViewModel> Items { get; set; } = new();
    }
}