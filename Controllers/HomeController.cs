using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;
using System.Linq;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private BankAccountsContext dbContext;
        public HomeController( BankAccountsContext context)
        {
            dbContext = context;
        }
        ///////////////////////////////////////////// GET HOME PAGE /////////////////////////

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        ///////////////////////////////////////////// POST REGISTRATION /////////////////////////

        [HttpPost]
        [Route("registration")]
        public IActionResult Registration(User UserSubmission)
        {
            System.Console.WriteLine("*************************** Enter Registration Function ***************************");
            if (ModelState.IsValid)
            {
                System.Console.WriteLine("*************************** Is Valid ***************************");
                if(dbContext.User.Any(u => u.Email == UserSubmission.Email))
                {
                    System.Console.WriteLine("*************************** Email already in use error ***************************");
                    ModelState.AddModelError("Email","Email already in use!");
                    return View("Index");
                }
                else
                {
                    System.Console.WriteLine("*************************** Success, Hashing password ***************************");
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    UserSubmission.Password = Hasher.HashPassword(UserSubmission, UserSubmission.Password);
                    HttpContext.Session.SetInt32("UserId", UserSubmission.UserId);
                    dbContext.Add(UserSubmission);
                    dbContext.SaveChanges();
                    return RedirectToAction("accountpage");
                }
            }
            System.Console.WriteLine("*************************** ModelState is not valid ***************************");
            ModelState.AddModelError("Email", "Invalid Email/Password");
            return View("Index");
        }

        ///////////////////////////////////////////// GET LOGIN PAGE /////////////////////////

        [HttpGet]
        [Route("loginpage")]
        public IActionResult LoginPage()
        {
            return View();
        }
        ///////////////////////////////////////////// POST LOGIN /////////////////////////

        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginUser UserSubmission)
        {
            System.Console.WriteLine("*************************** Enter Registration Function ***************************");
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.User.FirstOrDefault(u => u.Email == UserSubmission.Email);
                if(userInDb == null)
                {
                    System.Console.WriteLine("*************************** Email not in Database error ***************************");
                    ModelState.AddModelError("Email", "Email not in Database");
                    return View("loginpage");
                }            
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(UserSubmission, userInDb.Password, UserSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Email","not your Email!");
                    return View("loginpage");
                }
                System.Console.WriteLine("*************************** Success, Hashing password ***************************");
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                return RedirectToAction("AccountPage");
            }
                ModelState.AddModelError("Email", "Invalid Email/Password");
                System.Console.WriteLine("*************************** Not Valid ***************************");
                return View("loginpage");
        }

        ///////////////////////////////////////////// GET ACCOUNT PAGE /////////////////////////

        [HttpGet]
        [Route("accountpage")]
        public IActionResult AccountPage()
        {
            if(HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                
                System.Console.WriteLine("*************************** User in session ***************************");
                ViewBag.Transaction= dbContext.Transaction.Where(a =>a.UserId == HttpContext.Session.GetInt32("UserId"));
                User User=dbContext.User.FirstOrDefault( u => u.UserId == HttpContext.Session.GetInt32("UserId"));
                ViewBag.User = User;
                return View();
            }
        }
        ///////////////////////////////////////////// POST TRANSACTION /////////////////////////

        [HttpPost]
        [Route("transactions")]
        public IActionResult Transactions(Transaction transaction)
        {
            dbContext.Add(transaction);
            dbContext.SaveChanges();
            return RedirectToAction("AccountPage");
        }



    }
}
