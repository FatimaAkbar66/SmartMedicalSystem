using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Models.Entities;

namespace SmartMedicalSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ── DbSets ──────────────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<DrugInteraction> DrugInteractions { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<ValidationAlert> ValidationAlerts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Fix ALL cascade delete cycles ─────────────
            modelBuilder.Entity<DrugInteraction>()
                .HasOne(d => d.Drug1)
                .WithMany()
                .HasForeignKey(d => d.Drug1ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DrugInteraction>()
                .HasOne(d => d.Drug2)
                .WithMany()
                .HasForeignKey(d => d.Drug2ID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Fix Prescription cascade paths ────────────
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany(d => d.Prescriptions)
                .HasForeignKey(p => p.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(p => p.Prescriptions)
                .HasForeignKey(p => p.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ValidationAlert>()
                .HasOne(a => a.Prescription)
                .WithMany(p => p.ValidationAlerts)
                .HasForeignKey(a => a.PrescriptionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(i => i.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PrescriptionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalHistory>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(m => m.PatientID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Allergy>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Allergies)
                .HasForeignKey(a => a.PatientID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SEED: Users ────────────────────────────────
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Name = "System Admin",
                    Email = "admin@smartmedical.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    UserID = 2,
                    Name = "Dr. Sarah Ahmed",
                    Email = "doctor@smartmedical.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    UserID = 3,
                    Name = "Ali Hassan",
                    Email = "patient@smartmedical.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new User
                {
                    UserID = 4,
                    Name = "Raza Khan",
                    Email = "pharmacy@smartmedical.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacy@123"),
                    Role = "Pharmacist",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // ── SEED: Doctor ───────────────────────────────
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    DoctorID = 1,
                    UserID = 2,
                    Specialization = "Internal Medicine",
                    LicenseNo = "PMC-2024-001",
                    Department = "General Medicine",
                    Phone = "0300-1234567",
                    ExperienceYears = 10
                }
            );

            // ── SEED: Patient ──────────────────────────────
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientID = 1,
                    UserID = 3,
                    DateOfBirth = new DateTime(1985, 3, 15),
                    Gender = "Male",
                    BloodGroup = "A+",
                    Weight = 75,
                    Height = 175,
                    Phone = "0311-1234567",
                    EmergencyContact = "Fatima Hassan",
                    EmergencyPhone = "0300-9876543",
                    RegisteredAt = new DateTime(2024, 1, 1)
                }
            );

            // ── SEED: Allergies ────────────────────────────
            modelBuilder.Entity<Allergy>().HasData(
                new Allergy
                {
                    AllergyID = 1,
                    PatientID = 1,
                    Allergen = "Penicillin",
                    Severity = "Severe",
                    Reaction = "Anaphylaxis",
                    DateDocumented = new DateTime(2024, 1, 1)
                },
                new Allergy
                {
                    AllergyID = 2,
                    PatientID = 1,
                    Allergen = "Sulfa",
                    Severity = "Moderate",
                    Reaction = "Skin rash and hives",
                    DateDocumented = new DateTime(2024, 1, 1)
                }
            );

            // ── SEED: Medical History ──────────────────────
            modelBuilder.Entity<MedicalHistory>().HasData(
                new MedicalHistory
                {
                    HistoryID = 1,
                    PatientID = 1,
                    Diagnosis = "Hypertension Stage 1",
                    ChronicConditions = "Hypertension,Type 2 Diabetes",
                    SurgicalHistory = "Appendectomy 2010",
                    CurrentMedications = "Metformin 500mg daily",
                    DateRecorded = new DateTime(2024, 1, 1),
                    Notes = "Well controlled with medication"
                }
            );

            // ── SEED: Medicines ────────────────────────────
            modelBuilder.Entity<Medicine>().HasData(
                new Medicine
                {
                    MedicineID = 1,
                    GenericName = "Aspirin",
                    BrandNames = "Bayer,Disprin,Ecotrin",
                    ActiveIngredient = "Acetylsalicylic Acid",
                    Category = "NSAID/Antiplatelet",
                    MaxDosageMg = 1000,
                    MinDosageMg = 75,
                    AgeGroup = "Adult",
                    Contraindications = "Bleeding disorders,Aspirin allergy",
                    SideEffects = "GI bleeding,Stomach upset",
                    RequiresPrescription = false,
                    StockQuantity = 500,
                    ReorderLevel = 50,
                    Price = 5.00m
                },
                new Medicine
                {
                    MedicineID = 2,
                    GenericName = "Warfarin",
                    BrandNames = "Coumadin,Jantoven",
                    ActiveIngredient = "Warfarin Sodium",
                    Category = "Anticoagulant",
                    MaxDosageMg = 10,
                    MinDosageMg = 1,
                    AgeGroup = "Adult",
                    Contraindications = "Active bleeding,Pregnancy",
                    SideEffects = "Bleeding,Bruising",
                    RequiresPrescription = true,
                    StockQuantity = 200,
                    ReorderLevel = 20,
                    Price = 25.00m
                },
                new Medicine
                {
                    MedicineID = 3,
                    GenericName = "Paracetamol",
                    BrandNames = "Tylenol,Panadol,Calpol",
                    ActiveIngredient = "Acetaminophen",
                    Category = "Analgesic/Antipyretic",
                    MaxDosageMg = 1000,
                    MinDosageMg = 250,
                    AgeGroup = "All",
                    Contraindications = "Liver disease",
                    SideEffects = "Rare at normal doses",
                    RequiresPrescription = false,
                    StockQuantity = 800,
                    ReorderLevel = 100,
                    Price = 3.00m
                },
                new Medicine
                {
                    MedicineID = 4,
                    GenericName = "Amoxicillin",
                    BrandNames = "Amoxil,Trimox",
                    ActiveIngredient = "Amoxicillin",
                    Category = "Antibiotic/Penicillin",
                    MaxDosageMg = 500,
                    MinDosageMg = 125,
                    AgeGroup = "All",
                    Contraindications = "Penicillin allergy",
                    SideEffects = "Diarrhea,Nausea,Rash",
                    RequiresPrescription = true,
                    StockQuantity = 300,
                    ReorderLevel = 30,
                    Price = 12.00m
                },
                new Medicine
                {
                    MedicineID = 5,
                    GenericName = "Metformin",
                    BrandNames = "Glucophage,Fortamet",
                    ActiveIngredient = "Metformin HCl",
                    Category = "Antidiabetic/Biguanide",
                    MaxDosageMg = 2000,
                    MinDosageMg = 500,
                    AgeGroup = "Adult",
                    Contraindications = "Renal failure,Liver disease",
                    SideEffects = "GI upset,Lactic acidosis",
                    RequiresPrescription = true,
                    StockQuantity = 400,
                    ReorderLevel = 40,
                    Price = 8.00m
                },
                new Medicine
                {
                    MedicineID = 6,
                    GenericName = "Atorvastatin",
                    BrandNames = "Lipitor,Torvast",
                    ActiveIngredient = "Atorvastatin Calcium",
                    Category = "Statin/Lipid-lowering",
                    MaxDosageMg = 80,
                    MinDosageMg = 10,
                    AgeGroup = "Adult",
                    Contraindications = "Liver disease,Pregnancy",
                    SideEffects = "Muscle pain,Liver enzyme elevation",
                    RequiresPrescription = true,
                    StockQuantity = 250,
                    ReorderLevel = 25,
                    Price = 20.00m
                },
                new Medicine
                {
                    MedicineID = 7,
                    GenericName = "Lisinopril",
                    BrandNames = "Zestril,Prinivil",
                    ActiveIngredient = "Lisinopril",
                    Category = "ACE Inhibitor",
                    MaxDosageMg = 40,
                    MinDosageMg = 5,
                    AgeGroup = "Adult",
                    Contraindications = "Pregnancy,ACE inhibitor allergy",
                    SideEffects = "Dry cough,Hyperkalemia",
                    RequiresPrescription = true,
                    StockQuantity = 300,
                    ReorderLevel = 30,
                    Price = 15.00m
                },
                new Medicine
                {
                    MedicineID = 8,
                    GenericName = "Ibuprofen",
                    BrandNames = "Advil,Motrin,Nurofen",
                    ActiveIngredient = "Ibuprofen",
                    Category = "NSAID",
                    MaxDosageMg = 800,
                    MinDosageMg = 200,
                    AgeGroup = "Adult",
                    Contraindications = "Peptic ulcer,Renal impairment",
                    SideEffects = "GI bleeding,Kidney stress",
                    RequiresPrescription = false,
                    StockQuantity = 600,
                    ReorderLevel = 60,
                    Price = 6.00m
                },
                new Medicine
                {
                    MedicineID = 9,
                    GenericName = "Omeprazole",
                    BrandNames = "Prilosec,Losec",
                    ActiveIngredient = "Omeprazole",
                    Category = "Proton Pump Inhibitor",
                    MaxDosageMg = 40,
                    MinDosageMg = 10,
                    AgeGroup = "All",
                    Contraindications = "None significant",
                    SideEffects = "Headache,Diarrhea",
                    RequiresPrescription = false,
                    StockQuantity = 350,
                    ReorderLevel = 35,
                    Price = 10.00m
                },
                new Medicine
                {
                    MedicineID = 10,
                    GenericName = "Ciprofloxacin",
                    BrandNames = "Cipro,Proquin",
                    ActiveIngredient = "Ciprofloxacin HCl",
                    Category = "Antibiotic/Fluoroquinolone",
                    MaxDosageMg = 750,
                    MinDosageMg = 250,
                    AgeGroup = "Adult",
                    Contraindications = "Quinolone allergy",
                    SideEffects = "Tendon rupture,Nausea",
                    RequiresPrescription = true,
                    StockQuantity = 200,
                    ReorderLevel = 20,
                    Price = 18.00m
                },
                new Medicine
                {
                    MedicineID = 11,
                    GenericName = "Digoxin",
                    BrandNames = "Lanoxin,Digitek",
                    ActiveIngredient = "Digoxin",
                    Category = "Cardiac Glycoside",
                    MaxDosageMg = 1,
                    MinDosageMg = 1,
                    AgeGroup = "Adult",
                    Contraindications = "Ventricular fibrillation",
                    SideEffects = "Bradycardia,Nausea",
                    RequiresPrescription = true,
                    StockQuantity = 100,
                    ReorderLevel = 10,
                    Price = 30.00m
                },
                new Medicine
                {
                    MedicineID = 12,
                    GenericName = "Clopidogrel",
                    BrandNames = "Plavix,Iscover",
                    ActiveIngredient = "Clopidogrel Bisulfate",
                    Category = "Antiplatelet",
                    MaxDosageMg = 75,
                    MinDosageMg = 75,
                    AgeGroup = "Adult",
                    Contraindications = "Active bleeding",
                    SideEffects = "Bleeding,Bruising",
                    RequiresPrescription = true,
                    StockQuantity = 150,
                    ReorderLevel = 15,
                    Price = 35.00m
                }
            );

            // ── SEED: Drug Interactions ────────────────────
            modelBuilder.Entity<DrugInteraction>().HasData(
                new DrugInteraction
                {
                    InteractionID = 1,
                    Drug1ID = 1,
                    Drug2ID = 2,
                    Severity = "Severe",
                    Description = "Aspirin + Warfarin increases bleeding risk",
                    ClinicalEffect = "Major hemorrhage risk",
                    ManagementAdvice = "Avoid combination. Monitor INR."
                },
                new DrugInteraction
                {
                    InteractionID = 2,
                    Drug1ID = 1,
                    Drug2ID = 8,
                    Severity = "Moderate",
                    Description = "Aspirin + Ibuprofen reduces cardioprotection",
                    ClinicalEffect = "Reduced antiplatelet efficacy",
                    ManagementAdvice = "Use Paracetamol instead"
                },
                new DrugInteraction
                {
                    InteractionID = 3,
                    Drug1ID = 2,
                    Drug2ID = 6,
                    Severity = "Moderate",
                    Description = "Warfarin + Atorvastatin increases warfarin effect",
                    ClinicalEffect = "Elevated INR, bleeding risk",
                    ManagementAdvice = "Monitor INR closely"
                },
                new DrugInteraction
                {
                    InteractionID = 4,
                    Drug1ID = 1,
                    Drug2ID = 12,
                    Severity = "Severe",
                    Description = "Aspirin + Clopidogrel dual antiplatelet",
                    ClinicalEffect = "Severe GI bleeding risk",
                    ManagementAdvice = "Use only post-ACS with gastroprotection"
                },
                new DrugInteraction
                {
                    InteractionID = 5,
                    Drug1ID = 7,
                    Drug2ID = 5,
                    Severity = "Mild",
                    Description = "Lisinopril + Metformin hypoglycemia risk",
                    ClinicalEffect = "Hypoglycemia risk",
                    ManagementAdvice = "Monitor blood glucose"
                },
                new DrugInteraction
                {
                    InteractionID = 6,
                    Drug1ID = 8,
                    Drug2ID = 7,
                    Severity = "Moderate",
                    Description = "Ibuprofen reduces Lisinopril effect",
                    ClinicalEffect = "Reduced BP control",
                    ManagementAdvice = "Avoid NSAIDs with ACE inhibitors"
                },
                new DrugInteraction
                {
                    InteractionID = 7,
                    Drug1ID = 11,
                    Drug2ID = 6,
                    Severity = "Moderate",
                    Description = "Digoxin + Atorvastatin toxicity risk",
                    ClinicalEffect = "Digoxin toxicity",
                    ManagementAdvice = "Monitor digoxin levels"
                }
            );
        }
    }
}