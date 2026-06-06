using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartMedicalSystem.Controllers
{
    public class HomeController : Controller
    {
        // Redirect logged-in users to their dashboard
        [Authorize]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Doctor"))
                return RedirectToAction("Index", "Doctor");
            if (User.IsInRole("Pharmacist"))
                return RedirectToAction("Index", "Pharmacy");
            if (User.IsInRole("Patient"))
                return RedirectToAction("Index", "Patient");

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult Error() => View();
    }
}