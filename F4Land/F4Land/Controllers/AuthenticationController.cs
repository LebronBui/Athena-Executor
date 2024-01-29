using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using RealEstateAuction.DAL;
using RealEstateAuction.Models;
using RealEstateAuction.Services;
using System.Security.Claims;

namespace RealEstateAuction.Controllers
{
    //authenlogin
    //
    public class AuthenticationController : Controller
    {
        UserDAO userDAO = new UserDAO();
        private readonly IEmailSender _emailSender;

        public AuthenticationController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login()
        {
            string email = Request.Form["email"];
            string password = Request.Form["pwd"];

            var user = userDAO.GetUserByEmailAndPassword(email, password);

            Console.WriteLine(user);
            if (user != null)
            {
                List<Claim> claims = new List<Claim>() {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("FullName", user.FullName),
                    new Claim("Email", user.Email),
                    new Claim("Phone", user.Phone),
                    new Claim("Dob", user.Dob.ToString()),
                    new Claim("Address", user.Address),
                    new Claim("Avatar", user.Avatar==null?"":"oke"),
                    new Claim("RoleId", user.RoleId.ToString()),
                    new Claim("Description", user.Description==null?"":"oke"),
                };


                ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


                AuthenticationProperties properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    /*ExpiresUtc = System.DateTimeOffset.UtcNow.AddMinutes(20),*/

                    //Keep login
                    /*IsPersistent = true,*/
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                          new ClaimsPrincipal(claimsIdentity), properties);

                TempData["Message"] = "Login successful!";
                return Redirect("home");
            }
            else
            {
                TempData["Message"] = "Login fail!";
                return Redirect("home");
            }
        }

        [HttpPost("register")]
        public IActionResult Register()
        {
            string fullName = Request.Form["fullName"];
            string email = Request.Form["email"];
            string pwd = Request.Form["pwd"];
            string phone = Request.Form["phone"];
            string date = Request.Form["dob"];
            string address = Request.Form["address"];

            User user = new User()
            {
                FullName = fullName,
                Email = email,
                Password = pwd,
                Phone = phone,
                Dob = DateTime.Parse(date),
                Address = address,
                //set role id of member is 3
                RoleId = 3,
                Wallet = 0
            };

            var exist = userDAO.GetUserByEmail(email);
            if (exist != null)
            {
                TempData["Message"] = "Email already exists!";
                ViewBag.User = user;
            }
            else
            {
                var result = userDAO.AddUser(user);
                if (result)
                {
                    TempData["Message"] = "Register successful!";
                }
                else
                {
                    TempData["Message"] = "Register fail!";
                }
            }

            return Redirect("home");
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword()
        {
            var user = userDAO.GetUserByEmail(Request.Form["email"]);
            if (user == null)
            {
                TempData["Message"] = "Email does not exist!";
                return Redirect("home");
            }
            else
            {
                string email = Request.Form["email"];

                // Generate a random OTP (6-digit number)
                Random random = new Random();
                int otpNumber = random.Next(0, 1000000);
                string otp = otpNumber.ToString("D6");

                // Save the OTP into session
                HttpContext.Session.SetString("Otp", otp);

                // Save user email into session
                HttpContext.Session.SetString("email", email);

                //send email
                _emailSender.SendEmailAsync(email, "Reset your password!",
                    "\nEnter OTP to change your password!" +
                    "\nYour OTP: " + otp);

                //redirect to enter otp page
                return Redirect("enter-otp");
            }
        }

        [HttpGet("enter-otp")]
        public IActionResult EnterOtp()
        {
            return View();
        }

        [HttpPost("enter-otp")]
        public IActionResult EnterOtp(IFormCollection formCollection)
        {
            string otp = formCollection["otp"];
            //check otp is correct or not
            if (HttpContext.Session.GetString("Otp").Equals(otp))
            {
                HttpContext.Session.Remove("Otp");
                return Redirect("reset-password");
            }
            else
            {
                TempData["Message"] = "Wrong OTP!";
                return View();
            }
        }

        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(IFormCollection formCollection)
        {
            string pwd = formCollection["pwd"];
            userDAO.UpdatePassword(HttpContext.Session.GetString("email"), pwd);

            TempData["Message"] = "Reset password successful!";
            return Redirect("home");
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("home");
        }
    }
}
