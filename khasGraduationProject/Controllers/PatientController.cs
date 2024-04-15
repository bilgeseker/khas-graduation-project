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
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            using (var context = new WebContext())
            {
                // Tüm hastaları liste olarak al
                List<PatientDetails> modelList = context.patients.ToList();

                // Modeli view'e geçir
                return View(modelList);
            }
        }

        public IActionResult SignUp()
        {
            return View();
        }
    }
}

