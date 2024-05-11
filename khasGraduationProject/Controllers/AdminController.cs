using khasGraduationProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace khasGraduationProject.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Home()
        {
            var adminId = HttpContext.Session.GetString("adminId");

            if (adminId == null)
            {
                return View("Login");
            }
            else
            {
                var context = new WebContext();
                var doctors = context.doctors.ToList();

                return View(doctors);
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home"); //Home controllerdeki indexe atayacak.
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
            var user = context.admins.FirstOrDefault(u => u.email == email);

            if (user == null || !VerifyPassword(user.password, password))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View("Login");
            }

            HttpContext.Session.SetString("adminId", user.id.ToString());
            return RedirectToAction("Home");

        }

        private bool VerifyPassword(string password, string inputPassword)
        {
            var hashedPass = HashPass(inputPassword);
            return password == hashedPass;
        }

        [HttpPost]
        public IActionResult AdminChangePassword(string currentPassword, string newPassword, string newPasswordAgain)
        {
            using (var context = new WebContext())
            {
                var adminUser = context.admins.FirstOrDefault(u => u.id == Convert.ToInt32(HttpContext.Session.GetString("adminId")));

                if (adminUser != null)
                {
                    if (adminUser.password.Equals(HashPass(currentPassword)))
                    {
                        if (newPassword.Equals(newPasswordAgain))
                        {
                            adminUser.password = HashPass(newPassword);

                            context.admins.Update(adminUser);
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

                return RedirectToAction("Home");
            }
        }

        public IActionResult SignUp()
        {
            using (var context = new WebContext())
            {
                List<States> states = context.states.ToList();
                List<Specializations> specialization = context.specialization.ToList();

                dynamic myModel = new ExpandoObject();
                myModel.States = states;
                myModel.Specialization = specialization;

                return View(myModel);
            }
        }

        [HttpPost]
        public IActionResult DoctorSignUp(string name, string surname, int gender_id,
            string email, string password, int location_id, int specialization_id, string address, string phone)
        {
            using (var context = new WebContext())
            {
                var user = context.doctors.FirstOrDefault(u => u.email == email);
                if (user == null)
                {
                    var hashedPass = HashPass(password);

                    var newDoctor = new DoctorDetails
                    {
                        name = name,
                        surname = surname,
                        specialization_id = specialization_id,
                        gender_id = gender_id,
                        email = email,
                        password = hashedPass,
                        states_id = location_id,
                        address = address,
                        phone = phone,
                        app_id = 0,
                        profileImgPath = "profileImages"
                    };
                    context.doctors.Add(newDoctor);
                    context.SaveChanges();

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The user exist!!");
                    List<States> states = context.states.ToList();
                    return View("SignUp", states);
                }
                return RedirectToAction("Home");
            }
        }
    }
}
