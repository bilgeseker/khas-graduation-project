using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using khasGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace khasGraduationProject.Controllers
{
    
    public class PatientController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PatientController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Home()
        {
            var userId = HttpContext.Session.GetString("userId");
            ViewBag.userId = userId;
            return View();
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("userId");

            if (userId == null)
            {
                return View("Login");
            }
            else
            {
                var context = new WebContext();
                var patient = context.patients.FirstOrDefault(u => u.id == Convert.ToInt32(userId));

                var states = context.states.ToList();
             
                var gender = context.gender.ToList();

                dynamic myModel = new ExpandoObject();
                myModel.Patient = patient;  
                myModel.States = states;                
                myModel.Gender = gender;

                return View(myModel);
            }            
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

        public string HashPass(string password)
        {

            byte[] encodedPassword = new UTF8Encoding().GetBytes(password);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            string encoded = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

            return encoded;//returns hashed version of password
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
            HttpContext.Session.SetString("userId", user.id.ToString());
            return RedirectToAction("Home");

        }
        private bool VerifyPassword(string password, string inputPassword)
        {

            var hashedPass = HashPass(inputPassword);
            return password == hashedPass;
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult PatientSignUp(string name, string surname, DateOnly birthday, int gender_id,
            string email, string password, int location_id)
        {
            using(var context = new WebContext())
            {
                var user = context.patients.FirstOrDefault(u => u.email == email);
                if (user == null)
                {
                    DateTime now = DateTime.Now;
                    DateTime dateTimeWithNowTime = new DateTime(birthday.Year, birthday.Month, birthday.Day, now.Hour, now.Minute, now.Second);
                    var hashedPass = HashPass(password);
                   
                    var newPatient = new PatientDetails
                    {
                        name = name,
                        surname = surname,
                        birthday = dateTimeWithNowTime,
                        gender_id = gender_id,
                        email = email,
                        password = hashedPass,
                        location_id = location_id,
                        app_id = 0,
                        doctor_id = 0
                    };
                    context.patients.Add(newPatient);
                    context.SaveChanges();

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The user exist!!");
                    List<States> states = context.states.ToList();
                    return View("SignUp", states);
                }
                return RedirectToAction("Login");
            }
        }

        public IActionResult SignUp()
        {
            using(var context = new WebContext())
            {
                List<States> states = context.states.ToList();
                return View(states);
            }
        }


        [HttpPost]
        public IActionResult PatientSaveChanges(string name, string surname, string email, string password,
           DateTime birthday, string states, string gender, IFormFile files)
        {
            using (var context = new WebContext())
            {
                var user = context.patients.FirstOrDefault(u => u.id == Convert.ToInt32(HttpContext.Session.GetString("userId")));

                if (user != null)
                {
                    if (!user.name.Equals(name))
                    {
                        user.name = name;
                    }

                    if (!user.surname.Equals(surname))
                    {
                        user.surname = surname;
                    }

                    if (!user.email.Equals(email))
                    {
                        user.email = email;
                    }

                    if (!user.password.Equals(password))
                    {
                        user.password = HashPass(password);
                    }

                    if (!user.birthday.Equals(birthday))
                    {
                        user.birthday = birthday;
                    }

                    if (!user.location_id.Equals(Convert.ToInt32(states)))
                    {
                        user.location_id = Convert.ToInt32(states);
                    }

                    if (!user.gender_id.Equals(Convert.ToInt32(gender)))
                    {
                        user.gender_id = Convert.ToInt32(gender);
                    }

                    if (files != null && files.Length > 0)
                    {
                        try
                        {
                            // wwwroot klasörünün fiziksel yolu
                            var webRootPath = _hostingEnvironment.WebRootPath;

                            // Dosyanın kaydedileceği yol
                            var filePath = Path.Combine(webRootPath, "profileImages", files.FileName);

                            // Dosyanın var olup olmadığını kontrol et
                            if (!System.IO.File.Exists(filePath))
                            {
                                // Dosyayı wwwroot klasörüne kaydet
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    files.CopyTo(stream);
                                }
                            }

                            if (!user.profileImgPath.Equals(Path.Combine("profileImages", files.FileName)))
                            {
                                user.profileImgPath = Path.Combine("profileImages", files.FileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Hata durumunda uygun yanıtı döndür
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }

                    context.patients.Update(user);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }


    }
}

