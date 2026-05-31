using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMedicalSystem.Models.Entities
{
    // ─────────────────────────────────────────
    // USER (Base for all roles)
    // ─────────────────────────────────────────
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Patient";
        // Roles: Doctor | Pharmacist | Admin | Patient

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }

    // ─────────────────────────────────────────
    // PATIENT
    // ─────────────────────────────────────────
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User? User { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Not Specified";
        public string BloodGroup { get; set; } = "Unknown";
        public double Weight { get; set; }   // kg
        public double Height { get; set; }   // cm
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }

    // ─────────────────────────────────────────
    // DOCTOR
    // ─────────────────────────────────────────
    public class Doctor
    {
        [Key]
        public int DoctorID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User? User { get; set; }

        public string Specialization { get; set; } = string.Empty;
        public string LicenseNo { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int ExperienceYears { get; set; }

        // Navigation
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }

    // ─────────────────────────────────────────
    // MEDICAL HISTORY
    // ─────────────────────────────────────────
    public class MedicalHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient? Patient { get; set; }

        public string Diagnosis { get; set; } = string.Empty;
        public string? ChronicConditions { get; set; }
        public string? SurgicalHistory { get; set; }
        public string? CurrentMedications { get; set; }
        public string? FamilyHistory { get; set; }
        public string? Notes { get; set; }

        public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────
    // ALLERGY
    // ─────────────────────────────────────────
    public class Allergy
    {
        [Key]
        public int AllergyID { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient? Patient { get; set; }

        public string Allergen { get; set; } = string.Empty;
        public string Severity { get; set; } = "Mild";
        // Mild | Moderate | Severe
        public string? Reaction { get; set; }
        public DateTime DateDocumented { get; set; } = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────
    // MEDICINE
    // ─────────────────────────────────────────
    public class Medicine
    {
        [Key]
        public int MedicineID { get; set; }

        public string GenericName { get; set; } = string.Empty;
        public string BrandNames { get; set; } = string.Empty; // comma-separated
        public string ActiveIngredient { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double MaxDosageMg { get; set; }
        public double MinDosageMg { get; set; }
        public string AgeGroup { get; set; } = "All";
        // All | Pediatric | Adult | Geriatric
        public string? Contraindications { get; set; }
        public string? SideEffects { get; set; }
        public bool RequiresPrescription { get; set; } = true;
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public DateTime? ExpiryDate { get; set; }
        public decimal Price { get; set; }
    }

    // ─────────────────────────────────────────
    // DRUG INTERACTION
    // ─────────────────────────────────────────
    public class DrugInteraction
    {
        [Key]
        public int InteractionID { get; set; }

        public int Drug1ID { get; set; }
        public int Drug2ID { get; set; }

        [ForeignKey("Drug1ID")]
        public Medicine? Drug1 { get; set; }

        [ForeignKey("Drug2ID")]
        public Medicine? Drug2 { get; set; }

        public string Severity { get; set; } = "Mild";
        // Mild | Moderate | Severe
        public string Description { get; set; } = string.Empty;
        public string? ClinicalEffect { get; set; }
        public string? ManagementAdvice { get; set; }
    }

    // ─────────────────────────────────────────
    // PRESCRIPTION
    // ─────────────────────────────────────────
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        public string PrescriptionCode { get; set; } = string.Empty;

        [ForeignKey("Doctor")]
        public int DoctorID { get; set; }
        public Doctor? Doctor { get; set; }

        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public Patient? Patient { get; set; }

        public DateTime DateIssued { get; set; } = DateTime.UtcNow;
        public string Diagnosis { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        // Active | Dispensed | Cancelled
        public string? Notes { get; set; }

        // AI fields
        public string AIRiskLevel { get; set; } = "Low";
        // Low | Medium | High
        public float AIRiskScore { get; set; }
        public bool HasAlerts { get; set; }

        // Navigation
        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
        public ICollection<ValidationAlert> ValidationAlerts { get; set; } = new List<ValidationAlert>();
    }

    // ─────────────────────────────────────────
    // PRESCRIPTION ITEM
    // ─────────────────────────────────────────
    public class PrescriptionItem
    {
        [Key]
        public int ItemID { get; set; }

        [ForeignKey("Prescription")]
        public int PrescriptionID { get; set; }
        public Prescription? Prescription { get; set; }

        [ForeignKey("Medicine")]
        public int MedicineID { get; set; }
        public Medicine? Medicine { get; set; }

        public double Dosage { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public string Route { get; set; } = "Oral";
    }

    // ─────────────────────────────────────────
    // VALIDATION ALERT
    // ─────────────────────────────────────────
    public class ValidationAlert
    {
        [Key]
        public int AlertID { get; set; }

        [ForeignKey("Prescription")]
        public int PrescriptionID { get; set; }
        public Prescription? Prescription { get; set; }

        public string AlertType { get; set; } = string.Empty;
        // DrugInteraction | Allergy | Dosage | Duplicate
        public string Severity { get; set; } = "Warning";
        // Info | Warning | Critical
        public string Message { get; set; } = string.Empty;
        public bool IsOverridden { get; set; }
        public string? OverrideReason { get; set; }
        public string? OverriddenBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────
    // AUDIT LOG
    // ─────────────────────────────────────────
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IPAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}