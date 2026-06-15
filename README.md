# 🏥 Smart Medical Error Prevention & Prescription Validation System

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-.NET_10-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![ML.NET](https://img.shields.io/badge/ML.NET-3.0.1-FF6B6B?style=flat&logo=microsoft)](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)
[![Entity Framework](https://img.shields.io/badge/EF_Core-9.0-6DB33F?style=flat)](https://docs.microsoft.com/en-us/ef/core/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat&logo=bootstrap)](https://getbootstrap.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC2927?style=flat&logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)

> **An AI-powered clinical decision support system** that eliminates preventable
> medication errors through real-time prescription validation, ML.NET risk prediction,
> and a 4-layer safety engine — built with ASP.NET Core MVC and C#.

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Problem Statement](#-problem-statement)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [ML.NET AI Model](#-mlnet-ai-model)
- [Validation Engine](#-4-layer-validation-engine)
- [Project Structure](#-project-structure)
- [Database Design](#-database-design)
- [Architecture](#-architecture)
- [How to Run](#-how-to-run)
- [Demo Accounts](#-demo-accounts)
- [Test Scenarios](#-test-scenarios)
- [C# Concepts Used](#-c-concepts-used)
- [Future Enhancements](#-future-enhancements)

---

## 🌟 Overview

The **Smart Medical Error Prevention System** is a full-stack web application
designed to support healthcare professionals in making safer prescribing decisions.
The system integrates machine learning with rule-based clinical validation to create
a comprehensive safety net for medication management.

The platform serves four distinct user roles — **Doctors**, **Patients**,
**Pharmacists**, and **Administrators** — each with a dedicated dashboard and
role-specific functionality.

---

## 🚨 Problem Statement

Medication errors are among the most common and preventable causes of patient harm
in healthcare systems worldwide.

| Statistic | Value |
|-----------|-------|
| Annual patient injuries from medication errors | **1.5 Million** (WHO) |
| Avoidable healthcare cost annually | **$42 Billion** |
| Errors caused by drug interactions | **~30%** of all errors |
| Errors from allergy conflicts | **~15%** of all errors |
| Preventable with automated checking | **Up to 70%** |

**Root Causes:**
- Manual prescription writing with no automated verification
- Doctor fatigue and high patient volume
- Incomplete knowledge of all drug interactions
- No real-time allergy cross-checking
- Duplicate prescriptions under different brand names

---

## ✨ Key Features

### 🤖 AI-Powered Risk Prediction
- Real ML.NET **FastTree Binary Classification** model
- Trained on 1,500 synthetic patient records (WHO guidelines)
- Predicts **Low / Medium / High** adverse event risk
- Risk probability score with contributing factors
- Personalized clinical recommendations per patient
- Model auto-trains on first run — no manual setup needed

### 🛡️ 4-Layer Safety Validation Engine
- **Layer 1 — Allergy Detection:** Cross-checks medicines against documented allergies
- **Layer 2 — Drug Interaction:** Screens all drug combinations against interaction database
- **Layer 3 — Dosage Validation:** Verifies dose ranges, age groups, pediatric/geriatric safety
- **Layer 4 — Duplicate Detection:** Identifies same active ingredient under different names
- Live AJAX validation — real-time results without page reload

### 👥 Role-Based Access Control (RBAC)
- Four distinct user roles with separate dashboards
- Cookie-based authentication with BCrypt password hashing
- Claims-based identity management
- Role-protected controllers with `[Authorize]` attributes

### 📋 Prescription Management
- Dynamic prescription builder with multiple medicines
- Real-time validation panel with AI risk assessment
- PDF prescription export with full alert history
- Prescription status tracking — Active / Dispensed / Cancelled
- Clinical override with mandatory justification

### 💊 Pharmacy Workflow
- Pending prescription queue with AI risk indicators
- One-click dispense with automatic stock deduction
- Low stock alerts and expiry date monitoring
- Complete inventory management

### 🔒 Admin Control Panel
- System-wide analytics with Chart.js visualizations
- User management with activate/deactivate controls
- Medicines database management
- Drug interactions database
- Complete system audit log with timestamps

### 📊 Additional Features
- Full audit trail for all system activities
- Emergency contact management for patients
- Medical history and chronic condition tracking
- Allergy registry with severity levels
- Responsive UI with professional healthcare design

---

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Language** | C# | .NET 10 |
| **Framework** | ASP.NET Core MVC | .NET 10 |
| **ORM** | Entity Framework Core | 9.0.0 |
| **Database** | SQL Server LocalDB | — |
| **AI / ML** | ML.NET FastTree | 3.0.1 |
| **Authentication** | Cookie Auth + BCrypt | — |
| **Password Hashing** | BCrypt.Net-Next | 4.0.3 |
| **PDF Generation** | iText7 | 8.0.3 |
| **Frontend** | Bootstrap | 5.3.2 |
| **Icons** | Bootstrap Icons | 1.11.3 |
| **Charts** | Chart.js | 4.4.0 |
| **Fonts** | Google Fonts (Inter) | — |
| **Version Control** | Git + GitHub | — |

---

## 🧠 ML.NET AI Model

### Algorithm
**FastTree Binary Classification** — a gradient boosted decision tree algorithm
that builds an ensemble of decision trees for accurate binary prediction.

### Training Pipeline

```
┌─────────────────────────────────────────────────────┐
│                  ML.NET PIPELINE                    │
├─────────────────────────────────────────────────────┤
│                                                     │
│  1. Generate Synthetic Data                         │
│     └─ 1,500 patient records                        │
│     └─ Based on WHO clinical risk guidelines        │
│                                                     │
│  2. Feature Engineering (12 features)               │
│     └─ Age, Weight, Medication Count                │
│     └─ Allergy Count, Chronic Conditions            │
│     └─ Has Diabetes, Heart Disease, Kidney Disease  │
│     └─ Has Liver Disease, Previous Adverse Events   │
│     └─ Is Elderly (≥65), Is Pediatric (≤12)         │
│                                                     │
│  3. Feature Concatenation → "Features" vector       │
│                                                     │
│  4. FastTree Binary Classification Training         │
│     └─ 100 trees, 20 leaves per tree                │
│     └─ Learning rate: 0.1                           │
│     └─ 80% Train / 20% Test split                   │
│                                                     │
│  5. Model Evaluation                                │
│     └─ Accuracy: ~85%+                              │
│     └─ AUC (Area Under ROC): ~88%+                  │
│                                                     │
│  6. Model Persistence                               │
│     └─ Saved to: MLModels/risk_model.zip            │
│     └─ Auto-loaded on subsequent runs               │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### Risk Classification Thresholds

| Probability | Risk Level | Color | Action |
|-------------|-----------|-------|--------|
| ≥ 70% | **HIGH RISK** | 🔴 Red | Enhanced monitoring required |
| 35% – 69% | **MEDIUM RISK** | 🟡 Amber | Careful monitoring advised |
| < 35% | **LOW RISK** | 🟢 Green | Standard protocol applies |

### Note on Training Data
Synthetic training data is generated using clinically validated risk scoring
formulas based on WHO guidelines. This approach is standard practice in
academic, research, and prototype healthcare AI systems where real patient
data requires institutional ethical approval.

---

## 🛡️ 4-Layer Validation Engine

```
Doctor Submits Prescription
           │
           ▼
┌──────────────────────┐
│   LAYER 1: ALLERGY   │──── CRITICAL ──► BLOCK
│  Check patient allergy│
│  records vs medicine  │──── WARNING ───► ALERT
└──────────┬───────────┘
           │ Pass
           ▼
┌──────────────────────┐
│  LAYER 2: DRUG       │──── CRITICAL ──► BLOCK
│  INTERACTION         │
│  Screen all drug     │──── WARNING ───► ALERT
│  combinations        │
└──────────┬───────────┘
           │ Pass
           ▼
┌──────────────────────┐
│  LAYER 3: DOSAGE     │──── CRITICAL ──► BLOCK
│  Validate dose range │
│  Age group check     │──── WARNING ───► ALERT
│  Pediatric/Geriatric │
└──────────┬───────────┘
           │ Pass
           ▼
┌──────────────────────┐
│  LAYER 4: DUPLICATE  │──── CRITICAL ──► BLOCK
│  Same active         │
│  ingredient check    │──── WARNING ───► ALERT
│  Same drug class     │
└──────────┬───────────┘
           │ All Clear
           ▼
    ✅ PRESCRIPTION SAFE
    (+ ML.NET Risk Score)
```

---

## 📁 Project Structure

```
SmartMedicalSystem/
│
├── 📁 Controllers/
│   ├── AccountController.cs        # Authentication - Login, Register, Logout
│   ├── AdminController.cs          # System management and analytics
│   ├── DoctorController.cs         # Doctor dashboard and patient management
│   ├── HomeController.cs           # Role-based routing and error handling
│   ├── PatientController.cs        # Patient health dashboard
│   ├── PharmacyController.cs       # Pharmacy workflow and inventory
│   └── PrescriptionController.cs   # Prescription CRUD and AJAX validation
│
├── 📁 Data/
│   └── ApplicationDbContext.cs     # EF Core DbContext with relationships and seed data
│
├── 📁 Models/
│   ├── 📁 Entities/
│   │   └── Entities.cs             # User, Patient, Doctor, Medicine, Prescription...
│   ├── 📁 ViewModels/
│   │   └── ViewModels.cs           # All MVC ViewModels for data transfer
│   └── 📁 MLModels/
│       └── RiskModels.cs           # ML.NET PatientRiskInput and Prediction classes
│
├── 📁 Services/
│   ├── AuditService.cs             # Activity logging service
│   ├── PrescriptionPdfService.cs   # HTML prescription generation
│   ├── PrescriptionValidationService.cs  # 4-layer validation engine
│   └── RiskPredictionService.cs    # ML.NET training and prediction
│
├── 📁 Views/
│   ├── 📁 Account/
│   │   ├── AccessDenied.cshtml
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── 📁 Admin/
│   │   ├── AddMedicine.cshtml
│   │   ├── AuditLog.cshtml
│   │   ├── DrugInteractions.cshtml
│   │   ├── Index.cshtml
│   │   ├── Medicines.cshtml
│   │   └── Users.cshtml
│   ├── 📁 Doctor/
│   │   ├── Index.cshtml
│   │   ├── PatientProfile.cshtml
│   │   └── Patients.cshtml
│   ├── 📁 Patient/
│   │   ├── AddAllergy.cshtml
│   │   ├── AddHistory.cshtml
│   │   ├── Index.cshtml
│   │   ├── NoProfile.cshtml
│   │   └── Prescriptions.cshtml
│   ├── 📁 Pharmacy/
│   │   ├── Index.cshtml
│   │   └── Inventory.cshtml
│   ├── 📁 Prescription/
│   │   ├── Create.cshtml
│   │   ├── Detail.cshtml
│   │   └── List.cshtml
│   └── 📁 Shared/
│       ├── _Layout.cshtml
│       └── Error.cshtml
│
├── 📁 wwwroot/
│   └── 📁 css/
│       └── site.css                # Custom healthcare UI design system
│
├── 📁 MLModels/                    # Auto-generated model files (git-ignored)
│
├── appsettings.json                # Connection strings and configuration
├── Program.cs                      # Application startup and service registration
└── README.md                       # This file
```

---

## 🗄️ Database Design

### Entity Relationship Overview

```
┌──────────┐         ┌──────────┐         ┌──────────────┐
│  Users   │────1:1──│ Patients │────1:N──│   Allergies  │
│          │         │          │         └──────────────┘
│ UserID   │         │ PatientID│         ┌──────────────┐
│ Name     │         │ DOB      │────1:N──│MedicalHistory│
│ Email    │         │ BloodGrp │         └──────────────┘
│ Role     │         │ Weight   │         ┌──────────────┐
│ Password │         │ Height   │────1:N──│Prescriptions │
└──────────┘         └──────────┘         │              │
     │                                    │ PrescID      │
     │1:1                                 │ DoctorID─────┼──┐
┌────▼─────┐                             │ PatientID    │  │
│ Doctors  │                             │ AIRiskLevel  │  │
│          │─────────────────────────────│ AIRiskScore  │  │
│ DoctorID │         1:N                 └──────┬───────┘  │
│ Spec     │                                    │          │
│ License  │                             ┌──────▼───────┐  │
└──────────┘                             │PrescriptionItem│ │
                                         │ MedicineID ──┼──┼──┐
┌──────────────────┐                     └──────────────┘  │  │
│  DrugInteraction │                     ┌──────────────┐  │  │
│                  │                     │ValidationAlert│  │  │
│ Drug1ID ─────────┼──┐                  │ AlertType    │◄─┘  │
│ Drug2ID ─────────┼──┤                  │ Severity     │     │
│ Severity         │  │  ┌──────────┐    └──────────────┘     │
│ Description      │  └─►│ Medicines│◄───────────────────────┘
└──────────────────┘     │          │
                         │ MedID    │
                         │ GenName  │
                         │ MaxDose  │
                         │ Category │
                         └──────────┘

┌─────────────┐
│  AuditLogs  │  (Independent activity log)
│             │
│ UserID      │
│ Action      │
│ Timestamp   │
└─────────────┘
```

### Seeded Data

| Table | Records |
|-------|---------|
| Users | 4 (Admin, Doctor, Patient, Pharmacist) |
| Medicines | 12 (Aspirin, Warfarin, Metformin, Amoxicillin...) |
| Drug Interactions | 7 (Aspirin+Warfarin, Ibuprofen+Lisinopril...) |
| Patient Allergies | 2 (Penicillin, Sulfa) |
| Medical History | 1 (Hypertension + Diabetes) |

---

## 🏗️ Architecture

```
┌────────────────────────────────────────────────────────────┐
│                        BROWSER                             │
│              Bootstrap 5.3 + Vanilla JS (AJAX)            │
└──────────────────────────┬─────────────────────────────────┘
                           │ HTTP / HTTPS Requests
┌──────────────────────────▼─────────────────────────────────┐
│                  PRESENTATION LAYER                        │
│              ASP.NET Core MVC Controllers                  │
│   Account | Doctor | Prescription | Patient | Admin | Pharmacy │
└──────────┬───────────────────────────────────┬─────────────┘
           │                                   │
┌──────────▼──────────┐           ┌────────────▼────────────┐
│    VIEWS (.cshtml)  │           │      BUSINESS LAYER     │
│                     │           │                         │
│ Razor Pages         │           │ RiskPredictionService   │
│ Bootstrap 5.3 UI    │           │ ValidationService       │
│ Chart.js            │           │ PrescriptionPdfService  │
│ AJAX Validation     │           │ AuditService            │
└─────────────────────┘           └────────────┬────────────┘
                                               │
┌──────────────────────────────────────────────▼────────────┐
│                      DATA LAYER                           │
│              Entity Framework Core 9.0                    │
│           ApplicationDbContext + Repositories             │
└──────────────────────────────────────────────┬────────────┘
                                               │
┌──────────────────────────────────────────────▼────────────┐
│                   SQL SERVER DATABASE                      │
│                     (LocalDB)                             │
│       Auto-created with EnsureCreated() on startup        │
└───────────────────────────────────────────────────────────┘
                                ▲
┌───────────────────────────────┴───────────────────────────┐
│                   ML.NET PIPELINE                         │
│     FastTree Model → risk_model.zip → Prediction Engine   │
│         Runs as Singleton Service on app startup          │
└───────────────────────────────────────────────────────────┘
```

---

## ⚙️ How to Run

### Prerequisites

```
✅ Visual Studio 2022 or later (recommended VS 2026)
✅ .NET 10 SDK
✅ SQL Server LocalDB (included with Visual Studio)
✅ Internet connection (for NuGet packages on first build)
```

### Installation Steps

**Step 1 — Clone the Repository**

```bash
git clone https://github.com/FatimaAkbar66/SmartMedicalSystem.git
cd SmartMedicalSystem
```

**Step 2 — Open in Visual Studio**

```
File → Open → Project/Solution
Navigate to SmartMedicalSystem folder
Select SmartMedicalSystem.sln
Click Open
```

**Step 3 — Restore NuGet Packages**

```
Right-click Solution → Restore NuGet Packages
OR
Tools → NuGet Package Manager → Package Manager Console
→ Type: dotnet restore
```

**Step 4 — Verify Connection String**

Open `appsettings.json` and confirm:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartMedicalDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Step 5 — Run the Application**

```
Press F5 (Debug) or Ctrl+F5 (Without Debug)
```

**What happens automatically on first run:**

```
1. SQL Server database SmartMedicalDB is created
2. All tables are created from entity models
3. Seed data is inserted:
   - 4 demo user accounts
   - 12 medicines
   - 7 drug interactions
   - Sample patient with allergies
4. ML.NET model trains on 1,500 synthetic records (~10 seconds)
5. Model saved to MLModels/risk_model.zip
6. Browser opens at Login page
```

---

## 🔐 Demo Accounts

| Role | Email | Password | Access |
|------|-------|----------|--------|
| **Admin** | admin@smartmedical.com | Admin@123 | Full system control |
| **Doctor** | doctor@smartmedical.com | Doctor@123 | Prescriptions + Patients |
| **Patient** | patient@smartmedical.com | Patient@123 | Own health data |
| **Pharmacist** | pharmacy@smartmedical.com | Pharmacy@123 | Dispense + Inventory |

---

## 🧪 Test Scenarios

### Test 1 — Drug Interaction Alert

```
1. Login: doctor@smartmedical.com / Doctor@123
2. Click: New Prescription
3. Select any patient
4. Enter diagnosis: "Test Case"
5. Add Medicine: Aspirin (dose: 500)
6. Add Medicine: Warfarin (dose: 5)
7. Click: Run AI Safety Validation

Expected Result:
🔴 CRITICAL - DRUG INTERACTION (SEVERE)
"Aspirin + Warfarin significantly increases bleeding risk"
System blocks finalization until override provided
```

### Test 2 — Allergy Conflict Alert

```
1. Login: doctor@smartmedical.com / Doctor@123
2. New Prescription → Select Patient: Ali Hassan
   (has documented Penicillin SEVERE allergy)
3. Add Medicine: Amoxicillin (any dose)
4. Click: Run AI Safety Validation

Expected Result:
🔴 CRITICAL - ALLERGY CONFLICT
"Amoxicillin belongs to Penicillin class.
Patient has documented Penicillin allergy."
System shows Anaphylaxis as known reaction
```

### Test 3 — Overdose Detection

```
1. New Prescription → any patient
2. Add Medicine: Paracetamol
3. Enter dose: 2000 (max is 1000mg)
4. Click: Run AI Safety Validation

Expected Result:
🔴 CRITICAL - OVERDOSE RISK
"Paracetamol dose 2000mg exceeds maximum
safe dose of 1000mg"
```

### Test 4 — Duplicate Medication

```
1. New Prescription → any patient
2. Add Medicine: Aspirin (dose: 100)
3. Add Medicine: Aspirin again (same medicine twice)
4. Click: Run AI Safety Validation

Expected Result:
🔴 CRITICAL - DUPLICATE MEDICATION
"Same active ingredient detected"
System blocks duplicate prescriptions
```

### Test 5 — ML.NET Risk Assessment

```
1. Login: doctor@smartmedical.com
2. Click: Patients → View Profile (Ali Hassan)
3. Observe AI Risk Assessment panel

Expected Result:
ML.NET model calculates risk based on:
- Patient age and weight
- Documented allergies (2)
- Chronic conditions (Hypertension + Diabetes)
- Current medications
Displays risk percentage with contributing factors
```

### Test 6 — Pharmacy Dispense

```
1. First create a valid prescription as Doctor
2. Logout → Login: pharmacy@smartmedical.com
3. Dashboard shows pending prescription
4. Click Dispense button

Expected Result:
✅ Prescription marked as "Dispensed"
✅ Medicine stock automatically reduced
✅ No longer shows in pending queue
```

---

## 💻 C# Concepts Used

```
Object-Oriented Programming
├── Classes and Objects          All entity models and services
├── Inheritance                  Base controller functionality
├── Encapsulation                Private service methods
└── Abstraction                  Service interfaces

Advanced C# Features
├── Async / Await                All controller actions and DB queries
├── LINQ                         All Entity Framework queries
├── Generics                     List<T>, ICollection<T> throughout
├── Lambda Expressions           LINQ predicates and selectors
├── Extension Methods            Custom helper methods
├── Nullable Reference Types     .NET 10 nullable annotations (?)
├── Pattern Matching             Switch expressions for risk levels
├── String Interpolation         $"..." used throughout
└── Exception Handling           Try-catch in critical service paths

ASP.NET Core MVC
├── Model Binding                Form data to ViewModel mapping
├── Data Annotations             [Required], [EmailAddress] validation
├── Tag Helpers                  asp-for, asp-action, asp-controller
├── Dependency Injection         All services via constructor injection
├── Routing                      Attribute and convention routing
├── Action Filters               [Authorize] attribute
├── JSON Serialization           AJAX requests and responses
└── Cookie Authentication        Claims-based identity

Entity Framework Core
├── Code First                   Models define database schema
├── Migrations                   EnsureCreated for auto-setup
├── Relationships                One-to-Many, navigation properties
├── LINQ Queries                 Include, Where, OrderBy, Select
└── Seed Data                    HasData() in OnModelCreating

ML.NET
├── Data Loading                 LoadFromEnumerable
├── Pipeline Building            Transforms + Trainer
├── Model Training               FastTree binary classification
├── Model Evaluation             Accuracy, AUC metrics
├── Model Persistence            Save and Load zip file
└── Prediction Engine            CreatePredictionEngine<T,T>
```

---

## 🔮 Future Enhancements

- [ ] **Mobile Application** — .NET MAUI cross-platform app
- [ ] **Real Drug Database** — Integration with DrugBank API
- [ ] **Barcode Scanner** — Medicine verification at dispensing
- [ ] **Email / SMS Alerts** — Notifications for high risk prescriptions
- [ ] **FHIR Integration** — Healthcare interoperability standards
- [ ] **Voice Input** — Speech-to-text prescription creation
- [ ] **Multi-Language** — Urdu language support
- [ ] **Telemedicine** — Video consultation integration
- [ ] **Advanced Analytics** — Prescription pattern analysis
- [ ] **Cloud Deployment** — Azure App Service hosting

---


## 📚 References

- [ML.NET Documentation](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [ASP.NET Core MVC Docs](https://docs.microsoft.com/en-us/aspnet/core/mvc/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [WHO Medication Safety](https://www.who.int/initiatives/medication-without-harm)
- [Bootstrap 5.3](https://getbootstrap.com/docs/5.3/)
- [DrugBank Drug Interactions](https://go.drugbank.com/)

---

## 📄 License

This project is developed for **academic purposes** 

<div align="center">

**Built with ❤️ using ASP.NET Core MVC + ML.NET**

*Smart Medical Error Prevention System — Protecting Patients Through AI*

</div>
