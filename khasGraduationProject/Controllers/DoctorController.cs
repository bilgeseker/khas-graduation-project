using khasGraduationProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace khasGraduationProject.Controllers
{

    public class DoctorController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DoctorController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
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
                var doctor = context.doctors.FirstOrDefault(u => u.id == Convert.ToInt32(userId));

                var states = context.states.ToList();
                var specializations = context.specialization.ToList();
                var gender = context.gender.ToList();

                dynamic myModel = new ExpandoObject();
                myModel.Doctor = doctor;
                myModel.States = states;
                myModel.Specializations = specializations;
                myModel.Gender = gender;

                return View(myModel);
            }
        }

        public IActionResult Home()
        {
            var userId = HttpContext.Session.GetString("userId");

            if (userId == null)
            {
                return View("Login");
            }
            else
            {
                var context = new WebContext();

                var appointments = context.appointments
                    .Where(x => x.doctor_id == Convert.ToInt32(userId))
                    .Join(
                        context.doctors,
                        appointment => appointment.doctor_id,
                        doctor => doctor.id,
                        (appointment, doctor) => new { Appointment = appointment, Doctor = doctor }
                    )
                    .Join(
                        context.patients,
                        result => result.Appointment.patient_id,
                        patient => patient.id,
                        (result, patient) => new AppointmentViewModel
                        {
                            AppointmentId = result.Appointment.id,
                            Date = result.Appointment.date,
                            Time = result.Appointment.time,
                            IsCancelled = result.Appointment.isCancelled,
                            DoctorName = result.Doctor.name,
                            DoctorSurname = result.Doctor.surname,
                            DoctorProfileImgPath = result.Doctor.profileImgPath,
                            DoctorEmail = result.Doctor.email,
                            DoctorPhone = result.Doctor.phone,
                            PatientName = patient.name,
                            PatientSurname = patient.surname,
                            PatientProfileImgPath = patient.profileImgPath,
                            PatientBirthday = patient.birthday,
                            PatientEmail = patient.email
                        })
                    .OrderByDescending(appointment => appointment.Date)
                    .ToList();

                return View(appointments);
            }
        }


        public IActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
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
            var user = context.doctors.FirstOrDefault(u => u.email == email);

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

        [HttpPost]
        public IActionResult DoctorSaveChanges(string name, string surname, string email, string password,
           string phone, string states, string gender, string specialization, IFormFile files)
        {
            using (var context = new WebContext())
            {
                var user = context.doctors.FirstOrDefault(u => u.id == Convert.ToInt32(HttpContext.Session.GetString("userId")));

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

                    if (!user.phone.Equals(phone))
                    {
                        user.phone = phone;
                    }

                    if (!user.states_id.Equals(Convert.ToInt32(states)))
                    {
                        user.states_id = Convert.ToInt32(states);
                    }

                    if (!user.gender_id.Equals(Convert.ToInt32(gender)))
                    {
                        user.gender_id = Convert.ToInt32(gender);
                    }

                    if (!user.specialization_id.Equals(Convert.ToInt32(specialization)))
                    {
                        user.specialization_id = Convert.ToInt32(specialization);
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

                    context.doctors.Update(user);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult DoctorChangePassword(string currentPassword, string newPassword, string newPasswordAgain)
        {
            string returnUrl = HttpContext.Request.Headers["Referer"].ToString();

            var uri = new Uri(returnUrl);

            string action = "Index";

            if (uri.Segments.Length == 2)
            {
                action = "Index";
            } 
            else
            {
                action = uri.Segments[2].Trim('/');
            }

            using (var context = new WebContext())
            {
                var user = context.doctors.FirstOrDefault(u => u.id == Convert.ToInt32(HttpContext.Session.GetString("userId")));

                if (user != null)
                {                    
                    if (user.password.Equals(HashPass(currentPassword)))
                    {
                        if (newPassword.Equals(newPasswordAgain))
                        {
                            user.password = HashPass(newPassword);

                            context.doctors.Update(user);
                            context.SaveChanges();
                        }
                        else
                        {
                            ModelState.AddModelError("newPasswordAgain", "Re-enter the new password.");
                        }
                    } 
                    else
                    {
                        ModelState.AddModelError("currentPassword", "Password Entered Does Not Equal Current Password.");
                    }
                }

                return RedirectToAction(action);
            }
        }
    }
}
