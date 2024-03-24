using Microsoft.AspNetCore.Mvc;

namespace khasGraduationProject.Controllers
{
    public class DoctorAppointmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
