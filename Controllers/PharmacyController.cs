using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMedicalSystem.Data;
using SmartMedicalSystem.Models.ViewModels;
using SmartMedicalSystem.Services;

namespace SmartMedicalSystem.Controllers
{
    [Authorize(Roles = "Pharmacist")]
    public class PharmacyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AuditService _audit;

        public PharmacyController(
            ApplicationDbContext db,
            AuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        // ─────────────────────────────────────────
        // GET: /Pharmacy/Index — Dashboard
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var model = new PharmacyDashboardViewModel
            {
                PendingPrescriptions = await _db.Prescriptions
                    .Include(p => p.Patient)
                        .ThenInclude(p => p!.User)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d!.User)
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Medicine)
                    .Include(p => p.ValidationAlerts)
                    .Where(p => p.Status == "Active")
                    .OrderByDescending(p => p.DateIssued)
                    .ToListAsync(),

                LowStockMedicines = await _db.Medicines
                    .Where(m => m.StockQuantity <= m.ReorderLevel)
                    .OrderBy(m => m.StockQuantity)
                    .ToListAsync(),

                ExpiringMedicines = await _db.Medicines
                    .Where(m => m.ExpiryDate.HasValue &&
                                m.ExpiryDate <=
                                DateTime.Today.AddDays(30))
                    .OrderBy(m => m.ExpiryDate)
                    .ToListAsync(),

                DispensedToday = await _db.Prescriptions
                    .CountAsync(p =>
                        p.Status == "Dispensed" &&
                        p.DateIssued.Date == today)
            };

            return View(model);
        }

        // ─────────────────────────────────────────
        // POST: /Pharmacy/Dispense/{id}
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Dispense(int id)
        {
            var prescription = await _db.Prescriptions
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);

            if (prescription == null)
                return NotFound();

            // Update status
            prescription.Status = "Dispensed";

            // Deduct stock for each medicine
            foreach (var item in prescription.Items)
            {
                var medicine = await _db.Medicines
                    .FindAsync(item.MedicineID);

                if (medicine != null)
                {
                    medicine.StockQuantity = Math.Max(
                        0,
                        medicine.StockQuantity - item.DurationDays);
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Prescription dispensed and stock updated.";
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────
        // GET: /Pharmacy/Inventory
        // ─────────────────────────────────────────
        public async Task<IActionResult> Inventory()
        {
            var medicines = await _db.Medicines
                .OrderBy(m => m.GenericName)
                .ToListAsync();
            return View(medicines);
        }

        // ─────────────────────────────────────────
        // POST: /Pharmacy/UpdateStock
        // ─────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateStock(
            int medicineId, int quantity)
        {
            var medicine = await _db.Medicines
                .FindAsync(medicineId);

            if (medicine != null)
            {
                medicine.StockQuantity = quantity;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Stock updated successfully.";
            }

            return RedirectToAction("Inventory");
        }
    }
}