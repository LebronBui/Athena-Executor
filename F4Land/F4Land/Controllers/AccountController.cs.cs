using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;
using System.Security.Claims;


namespace RealEstateAuction.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDAO userDAO = new UserDAO();
        private IMapper _mapper;

        public AccountController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            //check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Please login to view profile!";
                return RedirectToAction("Index", "Home");
            }

            string email = User.FindFirstValue("Email");
            User user = userDAO.GetUserByEmail(email);

            //map user to user data model
            UserDatalModel userData = _mapper.Map<User, UserDatalModel>(user);

            return View(userData);
        }

        [HttpPost]
        [Route("profile")]
        public IActionResult Profile(UserDatalModel userData)
        {
            if (!ModelState.IsValid)
            {
                return View(userData);
            }
            else
            {
                User user = _mapper.Map<UserDatalModel, User>(userData);
                //get old user
                User oldUser = userDAO.GetUserByEmail(user.Email);
                //update old user
                oldUser.FullName = user.FullName;
                oldUser.Phone = user.Phone;
                oldUser.Dob = user.Dob;
                oldUser.Address = user.Address;

                userDAO.UpdateUser(oldUser);
                TempData["Message"] = "Update profile successfully!";
                return View(userData);
            }
        }

        [HttpGet]
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Please login to change password!";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("change-password")]
        public IActionResult ChangePassword(ChangePasswordModel passwordData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                string email = User.FindFirstValue("Email");
                User user = userDAO.GetUserByEmail(email);
                if (!user.Password.Equals(passwordData.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu cũ không đúng!");
                    return View();
                }
                else
                {
                    userDAO.UpdatePassword(email, passwordData.NewPassword);

                    TempData["Message"] = "Change password successful!";
                    return View();
                }
            }
        }
    }
}
