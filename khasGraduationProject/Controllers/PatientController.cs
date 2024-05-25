using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using khasGraduationProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

            if (userId == null)
            {
                return View("Login");
            }
            else
            {
                var context = new WebContext();

                var appointments = context.appointments
                    .Where(x => x.patient_id == Convert.ToInt32(userId))
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
                    .OrderBy(appointment => appointment.IsCancelled)
                    .ThenByDescending(appointment => appointment.Date)
                    .ToList();

                var query = (
                                from a in context.audios
                                join p in context.patientsAudios on a.id equals p.audio_id
                                join app in context.appointments on p.app_id equals app.id
                                where p.percentage > 0 && app.patient_id == Convert.ToInt32(userId) //&& p.app_id == 8
                                select new AudioPatientViewModel
                                {
                                    AudioId = a.id,
                                    AudioFilePath = a.audioFilePath,
                                    AudioText = a.audioText,

                                    PatientsAudiosId = a.id,
                                    PatientsAudiosAudioId = p.audio_id,
                                    PatientsAudiosAppId = p.app_id,
                                    PatientAudioFilePath = p.patientAudioFilePath,
                                    PatientAudioText = p.patientAudioText,
                                    PatientAudioPercentage = p.percentage
                                }
                           ).ToList();

                dynamic myModel = new ExpandoObject();
                myModel.AppointmentViewModel = appointments;
                myModel.AudioPatientViewModel = query;

                return View(myModel);
            }
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
            
            return View();
        }

        public IActionResult Training(int id)
        {
            var userId = HttpContext.Session.GetString("userId");

            if (userId == null)
            {
                return View("Login");
            }
            else
            {
                var context = new WebContext();
                var audios = context.audios.ToList();
                var patientsAudios = context.patientsAudios.Where(x => x.app_id == id).ToList();

                var query = new List<AudioPatientViewModel>();

                if (patientsAudios.Count == 0)
                {
                    query = (
                                from a in context.audios
                                    join p in context.patientsAudios.Where(p => p.app_id == id) on a.id equals p.audio_id
                                into joined
                                from pAudio in joined.DefaultIfEmpty()
                                select new AudioPatientViewModel
                                {
                                    AudioId = a.id,
                                    AudioFilePath = a.audioFilePath,
                                    AudioText = a.audioText,

                                    PatientsAudiosId = pAudio != null ? pAudio.id : 0,
                                    PatientsAudiosAudioId = pAudio != null ? pAudio.audio_id : 0,
                                    PatientsAudiosAppId = pAudio != null ? pAudio.app_id : id,
                                    PatientAudioFilePath = pAudio != null ? pAudio.patientAudioFilePath : "",
                                    PatientAudioText = pAudio != null ? pAudio.patientAudioText : "",
                                    PatientAudioPercentage = pAudio != null ? pAudio.percentage : 0

                                }
                           ).ToList();
                } else
                {
                    query = (
                                from a in context.audios
                                join p in context.patientsAudios.Where(p => p.app_id == id) on a.id equals p.audio_id
                            into joined
                                from pAudio in joined.DefaultIfEmpty()
                                where pAudio != null //|| pAudio.app_id == id
                                select new AudioPatientViewModel
                                {
                                    AudioId = a.id,
                                    AudioFilePath = a.audioFilePath,
                                    AudioText = a.audioText,

                                    PatientsAudiosId = pAudio.id,
                                    PatientsAudiosAudioId = pAudio.audio_id,
                                    PatientsAudiosAppId = pAudio.app_id,
                                    PatientAudioFilePath = pAudio.patientAudioFilePath,
                                    PatientAudioText = pAudio.patientAudioText,
                                    PatientAudioPercentage = pAudio.percentage
                                }
                           ).ToList();
                }

                ViewBag.TrainingAppId = id;

                return View(query);
            }
        }

        [HttpPost]
        public IActionResult PatientsAudiosSave(int id, int appId, IFormFile files)
        {
            // id = audio_id
            using (var context = new WebContext())
            {
                var model = context.patientsAudios.FirstOrDefault(u => u.app_id == appId && u.audio_id == id);
                var audios = context.audios.ToList();
                var audioModel = context.audios.FirstOrDefault(u => u.id == id);

                if (model != null)
                {
                    if (files != null && files.Length > 0)
                    {
                        try
                        {
                            var webRootPath = _hostingEnvironment.WebRootPath;

                            var filePath = Path.Combine(webRootPath, "patientsAudiosFiles", files.FileName);

                            if (!System.IO.File.Exists(filePath))
                            {
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    files.CopyTo(stream);
                                }
                            }

                            string text = SpeechManager.AudioToText(filePath);
                            double calcPercentage = SpeechManager.CompareStrings(audioModel.audioText, text);

                            if (!model.patientAudioFilePath.Equals(Path.Combine("patientsAudiosFiles", files.FileName)))
                            {
                                model.patientAudioFilePath = Path.Combine("patientsAudiosFiles", files.FileName);
                            }

                            if (!model.patientAudioText.Equals(text))
                            {
                                model.patientAudioText = text;
                            }

                            if (!model.percentage.Equals(calcPercentage))
                            {
                                model.percentage = calcPercentage;
                            }

                            context.patientsAudios.Update(model);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }
                }
                else
                {
                    if (files != null && files.Length > 0)
                    {
                        try
                        {
                            var webRootPath = _hostingEnvironment.WebRootPath;

                            var filePath = Path.Combine(webRootPath, "patientsAudiosFiles", files.FileName);

                            if (!System.IO.File.Exists(filePath))
                            {
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    files.CopyTo(stream);
                                }
                            }

                            foreach (var item in audios)
                            {
                                var result = context.patientsAudios.Where(u => u.app_id == appId && u.audio_id == item.id).ToList().Count;

                                if (result == 0)
                                {
                                    if (item.id == id)
                                    {
                                        string text = SpeechManager.AudioToText(filePath);
                                        double calcPercentage = SpeechManager.CompareStrings(audioModel.audioText, text);

                                        var newPatientsAudios = new PatientsAudios
                                        {
                                            app_id = appId,
                                            audio_id = item.id,
                                            patientAudioFilePath = Path.Combine("patientsAudiosFiles", files.FileName),
                                            patientAudioText = text,
                                            percentage = calcPercentage
                                        };

                                        context.patientsAudios.Add(newPatientsAudios);
                                        context.SaveChanges();
                                    }
                                    else
                                    {
                                        var newPatientsAudios = new PatientsAudios
                                        {
                                            app_id = appId,
                                            audio_id = item.id,
                                            patientAudioFilePath = "",
                                            patientAudioText = "",
                                            percentage = 0
                                        };

                                        context.patientsAudios.Add(newPatientsAudios);
                                        context.SaveChanges();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }
                }

                return RedirectToAction("Training", new { id = appId });
            }
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
                        doctor_id = 0,
                        profileImgPath = "profileImages\\sample_patient.png"
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
        public IActionResult PatientSaveChanges(string name, string surname, string email,
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
                            var webRootPath = _hostingEnvironment.WebRootPath;

                            var filePath = Path.Combine(webRootPath, "profileImages", files.FileName);

                            if (!System.IO.File.Exists(filePath))
                            {
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
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }

                    context.patients.Update(user);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }

        public ActionResult Doctors()
        {
            using (var context = new WebContext())
            {
                ViewData["UserId"] = HttpContext.Session.GetString("userId");
                List<States> states = context.states.ToList();
                return View(states);
            }
        }

        [HttpGet]
        public IActionResult GetDoctorsByState(int stateId)
        {
            using (var context = new WebContext())
            {
                List<DoctorDetails> filteredDoctors = context.doctors.ToList().FindAll(u => u.states_id == stateId);
                return Json(filteredDoctors);
            }
        }

        [HttpPost]
        public IActionResult PatientChangePassword(string currentPassword, string newPassword, string newPasswordAgain)
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
                var user = context.patients.FirstOrDefault(u => u.id == Convert.ToInt32(HttpContext.Session.GetString("userId")));

                if (user != null)
                {
                    if (user.password.Equals(HashPass(currentPassword)))
                    {
                        if (newPassword.Equals(newPasswordAgain))
                        {
                            user.password = HashPass(newPassword);

                            context.patients.Update(user);
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

        [HttpPost]
        public IActionResult AppointmentCancel(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Home");

            using (var context = new WebContext())
            {
                var data = context.appointments.FirstOrDefault(u => u.id == Convert.ToInt32(id));

                if (data != null)
                {
                    data.isCancelled = true;

                    context.appointments.Update(data);
                    context.SaveChanges();
                }

                return RedirectToAction("Home");
            }
        }

        [HttpPost]
        public IActionResult CheckAppointmentAvailability(int doctorId, string selectedDate, string time, int user_id)
        {
            using(var context = new WebContext())
            {
                var formattedDate = DateTime.Parse(selectedDate);
                var formattedTime = TimeSpan.Parse(time);
                var data = context.appointments.FirstOrDefault(u => u.doctor_id == doctorId && 
                                                                u.date == formattedDate && 
                                                                u.time == formattedTime && 
                                                                u.patient_id == user_id && 
                                                                u.isCancelled == false);
                if (data != null) //&& data.isCancelled != true
                {
                    return Json(new { available = false });
                }
                else
                {
                    return Json(new { available = true });
                }
            }
            
        }

        [HttpGet]
        public IActionResult GetDoctorAndPatientInfo(int doctorId, int patientId)
        {
            using(var context = new WebContext())
            {
                var doctor = context.doctors.FirstOrDefault(d => d.id == doctorId);
                var patient = context.patients.FirstOrDefault(p => p.id == patientId);

                if (doctor != null && patient != null)
                {
                    return Json(new { doctor, patient });
                }
                else
                {
                    return Json(new { error = "Doctor or patient not found." });
                }
            }
        }

        [HttpPost]
        public IActionResult ApproveAppointment(int doctorId, int patientId, string date, string time)
        {
            using(var context = new WebContext())
            {
                var formattedDate = DateTime.Parse(date);
                var formattedTime = TimeSpan.Parse(time);

                var appointment = new Appointments
                {
                    date = formattedDate,
                    time = formattedTime,
                    doctor_id = doctorId,
                    patient_id = patientId,
                    isCancelled = false
                };
                try
                {
                    context.appointments.Add(appointment);
                    context.SaveChanges();
                    return Json(new { available = true });
                }
                catch (Exception ex)
                {
                    return Json(new { available = false, error = ex.Message });
                }
            }
        }
    }
}

