using khasGraduationProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;

namespace khasGraduationProject.Controllers
{
    public class DoctorController : Controller
    {
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
            return RedirectToAction("Index");

        }
        private bool VerifyPassword(string password, string inputPassword)
        {
            var hashedPass = HashPass(inputPassword);
            return password == hashedPass;
        }

        [HttpPost]
        public IActionResult DoctorSaveChanges(string name, string surname, string email, string password,
           string phone, string states, string gender, string specialization)
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

                    context.doctors.Update(user);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }
        }
    }
}
