using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Login_and_Registration.Models;
using Login_and_Registration.Persistence;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Login_and_Registration.Controllers
{
    public class HomeController : Controller
    {
        private Login_and_RegistrationDbContext dbContext;
        public HomeController(Login_and_RegistrationDbContext context) {
            dbContext = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Index(User user)
        {
            // Check initial ModelState
            if(ModelState.IsValid)
            {
                // If a User exists with provided email
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");
                    
                    // You may consider returning to the View at this point
                    return View();
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                
                return Redirect("/login");
            }

            // other code
            return View();
        } 

        [HttpGet]
        [Route("/login")]
        public IActionResult Login(){
            return View();
        }

        [HttpPost]
        [Route("/login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if(userInDb == null || userSubmission.Password == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View();
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // varify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    return View();
                }
                
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(userInDb));
                return Redirect("/success");
            }

            return View();
        }

        [Route("/success")]
        public IActionResult Success()
        {
            
            if(HttpContext.Session.GetString("User") != null) {
                ViewData["Message"] = "Your success page.";
                return View();
            } else {
                return Redirect("/");
            }
        }

        [HttpGet]
        public IActionResult Logout(){
                HttpContext.Session.SetString("User", null);

                return Redirect("/login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
