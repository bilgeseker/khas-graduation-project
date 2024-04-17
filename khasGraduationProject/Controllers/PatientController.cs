using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using khasGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace khasGraduationProject.Controllers
{
    
    public class PatientController : Controller
    {
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult Login()
        {
            //using (var context = new WebContext())
            //{
            //    // Tüm hastaları liste olarak al
            //    List<PatientDetails> modelList = context.patients.ToList();

            //    // Modeli view'e geçir
            //    return View(modelList);
            //}
            return View();
        }

        [HttpPost]
        public IActionResult Authenticate(string email, string password)
        {
            var context = new WebContext();
            var user = context.patients.FirstOrDefault(u => u.email == email);

            if (user == null || !VerifyPassword(user.password, password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View("Login");
            }

            return RedirectToAction("Home");

        }
        private bool VerifyPassword(string hashedPassword, string password)
        {
            return hashedPassword == password;
        }

        public IActionResult SignUp()
        {
            using(var context = new WebContext())
            {
                List<States> states = context.states.ToList();
                return View(states);
            }
        }
    }
}

