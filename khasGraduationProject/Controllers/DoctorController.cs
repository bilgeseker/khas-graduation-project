using Microsoft.AspNetCore.Mvc;

namespace khasGraduationProject.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
