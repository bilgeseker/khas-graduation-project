using khasGraduationProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace khasGraduationProject.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate(string email, string password)
        {
            var context = new WebContext();
            var user = context.doctors.FirstOrDefault(u => u.email == email);

            if (user == null || !VerifyPassword(user.password, password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View("Login");
            }

            return RedirectToAction("Index");

        }
        private bool VerifyPassword(string hashedPassword, string password)
        {
            return hashedPassword == password;
        }
    }
}
