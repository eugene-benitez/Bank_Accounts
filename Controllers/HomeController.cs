using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Bank_Accounts.Models;
using Microsoft.AspNetCore.Identity;

namespace Bank_Accounts.Controllers
{
    public class HomeController : Controller
    {

        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            DateTime CurrentTime = DateTime.Now;
            ViewBag.Now = CurrentTime;
            HttpContext.Session.Clear();
            return View();
        }


        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet("account/{userid}")]
        public IActionResult Account(int userid)
        {
            int? id = HttpContext.Session.GetInt32("LoggedId");
            User retrievedUser = dbContext.User.SingleOrDefault(u => u.UserId == id);
            // List<User> retrievedUser = dbContext.User.Where(u => u.UserId == id).ToList();
            @ViewBag.Id = id;
            @ViewBag.User = retrievedUser;
            List<Transactions> transactionsWithUser = dbContext.Transactions
            // populates each Message with its related User object (Creator)
            .Include(t => t.Transactor)
            .ToList();
            @ViewBag.transactions = transactionsWithUser;

            return View();
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        [Route("submit")]
        public IActionResult Submit(User NewUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.User.Any(u => u.Email == NewUser.Email))
                {

                    ModelState.AddModelError("Email", "Email already in use!");

                    return View("Index");
                }

                // Initializing a PasswordHasher object, providing our User class as its
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                NewUser.Password = Hasher.HashPassword(NewUser, NewUser.Password);
                //Save your user object to the database
                NewUser.Balance = 400m;
                dbContext.Add(NewUser);
                // OR dbContext.Users.Add(newUser);
                dbContext.SaveChanges();
                // Other code
                return RedirectToAction("login");
            }

            DateTime CurrentTime = DateTime.Now;
            ViewBag.Now = CurrentTime;
            return View("Index");
        }

        [HttpPost("log")]
        public IActionResult Log(LoginUser submission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.User.FirstOrDefault(u => u.Email == submission.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // varify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(submission, userInDb.Password, submission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
            }
            User retrievedUser = dbContext.User.FirstOrDefault(u => u.Email == submission.Email);
            int id = retrievedUser.UserId;
            HttpContext.Session.SetInt32("LoggedId", id);
            string url = $"account/{id}";
            return base.Redirect(url);
        }

        [HttpPost("use")]
        public IActionResult Use(Transactions newTransaction)
        {
            int? id = HttpContext.Session.GetInt32("LoggedId");
            @ViewBag.Id = id;
            User retrievedUser = dbContext.User.SingleOrDefault(u => u.UserId == id);
            string url = $"account/{id}";
            if (newTransaction.Amount > retrievedUser.Balance)
            {
                return base.Redirect(url);
            }
            DateTime CurrentTime = DateTime.Now;
            ViewBag.Now = CurrentTime;
            retrievedUser.Balance += newTransaction.Amount;
            dbContext.Add(newTransaction);
            dbContext.SaveChanges();
            return base.Redirect(url);
        }
    }
}
