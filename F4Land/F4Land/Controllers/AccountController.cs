using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using RealEstateAuction.Services;
using System;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;


namespace RealEstateAuction.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDAO userDAO;
        private readonly AuctionDAO auctionDAO;
        private IMapper _mapper;
        private Pagination pagination;

        public AccountController(IMapper mapper)
        {
            pagination = new Pagination();
            auctionDAO = new AuctionDAO();
            userDAO = new UserDAO();
            _mapper = mapper;
        }

        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            //check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sử dụng tính năng này!";
                return RedirectToAction("Index", "Home");
            }

            //Find User by Id
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = userDAO.GetUserById(id);

            //map User to UserDatalModel
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
                TempData["Message"] = "Cập nhật thông tin cá nhân thành công!";
                return View(userData);
            }
        }

        [HttpGet]
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để sử dụng tính năng này!";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("change-password")]
        public IActionResult ChangePassword(ChangePasswordModel passwordData)
        {
            //if value of user enter is not valid
            if (!ModelState.IsValid)
            {
                return View();
            }
            else
            {
                //find User by Id
                int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = userDAO.GetUserById(id);

                //verify old password of user
                if (!user.Password.Equals(passwordData.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu cũ không đúng!");
                    return View();
                }
                else
                {
                    //update new password
                    userDAO.UpdatePassword(user.Email, passwordData.NewPassword);
                    TempData["Message"] = "Thay đổi mật khẩu thành công!";
                    return View();
                }
            }
        }

        [HttpGet]
        [Route("manage-auction")]
        public IActionResult ManageAuction(int? pageNumber)
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }

            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (pageNumber.HasValue)
            {
                pagination.PageNumber = pageNumber.Value;
            }

            //get auction by user id
            List<Auction> auctions = auctionDAO.GetAuctionByUserId(userId, pagination);

            int auctionCount = auctionDAO.CountAuctionByUserId(userId);
            int pageSize = (int)Math.Ceiling((double)auctionCount / pagination.RecordPerPage);

            ViewBag.currentPage = pagination.PageNumber;
            ViewBag.pageSize = pageSize;

            return View(auctions);
        }

        [HttpGet]
        [Route("create-auction")]
        public IActionResult CreateAuction()
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [Route("create-auction")]
        public IActionResult CreateAuction([FromForm] AuctionDataModel auctionData)
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }
            //if value of user enter is not valid
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Vui lòng kiểm tra lại thông tin";
                ValidateAuction(auctionData);
                return View();
            }
            //if value of user enter is valid
            else
            {
                //validate the value of user enter
                bool validateModel = ValidateAuction(auctionData);
                if (validateModel)
                {
                    //get random staff to approve
                    User staff = userDAO.GetRandomStaff();

                    //get user by id
                    int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    User user = userDAO.GetUserById(userId);

                    auctionData.UserId = user.Id;
                    auctionData.Status = 1;
                    auctionData.ApproverId = staff.Id;
                    auctionData.Status = (int)AuctionStatus.Chờ_phê_duyệt;
                    auctionData.DeleteFlag = false;
                    auctionData.CreatedTime = DateTime.Now;


                    //create list Image
                    List<Image> images = new List<Image>();

                    foreach (var file in auctionData.ImageFiles)
                    {
                        //save image to folder and get url
                        var pathImage = FileUpload.UploadImageProduct(file);
                        if (pathImage != null)
                        {
                            Image image = new Image();
                            image.Url = pathImage;
                            images.Add(image);
                        }
                    }
                    //assign image to auction
                    auctionData.Images = images;

                    //map to Auction model
                    Auction auction = _mapper.Map<AuctionDataModel, Auction>(auctionData);
                    //add Auction to database
                    bool isSuccess = auctionDAO.AddAuction(auction);

                    //check if add acution successfull
                    if (isSuccess)
                    {
                        TempData["Message"] = "Tạo phiên đấu giá thành công!";
                        return Redirect("manage-auction");
                    }
                    else
                    {
                        TempData["Message"] = "Tạo phiên đấu giá thất bại!";
                        return View();
                    }
                }
                else
                {
                    TempData["Message"] = "Vui lòng kiểm tra lại thông tin";
                    return View();
                }
            }
        }


        [HttpGet]
        [Route("edit-auction")]
        public IActionResult EditAuction(int id)
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Find Auction by id
            Auction auction = auctionDAO.GetAuctionById(id);
            AuctionEditDataModel auctionData = _mapper.Map<Auction, AuctionEditDataModel>(auction);

            //check if auction belong to this user
            if (!(auction.UserId == userId))
            {
                TempData["Message"] = "Bạn không thể quản lý phiên đấu giá người khác!";
                return Redirect("manage-auction");
            }

            return View(auctionData);
        }

        [HttpPost]
        [Route("edit-auction")]
        public IActionResult EditAuction([FromForm] AuctionEditDataModel auctionData)
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }

            //get auction by id
            Auction auction = auctionDAO.GetAuctionById(auctionData.Id.Value);

            //if value of user enter is not valid
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Vui lòng kiểm tra lại thông tin";
                ValidateAuction(auctionData);

                auctionData.Images = auction.Images;
                return View(auctionData);
            }
            //if value of user enter is valid
            else
            {
                //validate the value of user enter
                bool validateModel = ValidateAuction(auctionData);
                if (validateModel)
                {

                    //update status of auction
                    auctionData.Status = (int)AuctionStatus.Chờ_phê_duyệt;
                    auctionData.UpdatedTime = DateTime.Now;

                    //update new information
                    auction.Title = auctionData.Title;
                    auction.StartPrice = auctionData.StartPrice;
                    auction.EndPrice = auctionData.EndPrice;
                    auction.Area = auctionData.Area;
                    auction.Address = auctionData.Address;
                    auction.Facade = auctionData.Facade;
                    auction.Direction = auctionData.Direction;
                    auction.StartTime = auctionData.StartTime;
                    auction.EndTime = auctionData.EndTime;

                    auction.Status = auctionData.Status.Value;
                    auction.UpdatedTime = auctionData.UpdatedTime;


                    //check user update image or not
                    if (!auctionData.ImageFiles.IsNullOrEmpty())
                    {
                        //create list Image
                        List<Image> images = new List<Image>();

                        foreach (var file in auctionData.ImageFiles)
                        {
                            //save image to folder and get url
                            var pathImage = FileUpload.UploadImageProduct(file);
                            if (pathImage != null)
                            {
                                Image image = new Image();
                                image.Url = pathImage;
                                images.Add(image);
                            }
                        }
                        auction.Images = images;
                    }

                    //update Auction to database
                    bool isSuccess = auctionDAO.EditAuction(auction);

                    //check if add acution successfull
                    if (isSuccess)
                    {
                        TempData["Message"] = "Cập nhật phiên đấu giá thành công!";
                        return Redirect("manage-auction");
                    }
                    else
                    {
                        TempData["Message"] = "Cập nhật phiên đấu giá thất bại!";

                        auctionData.Images = auction.Images;
                        return View(auctionData);
                    }
                }
                else
                {
                    TempData["Message"] = "Vui lòng kiểm tra lại thông tin";

                    auctionData.Images = auction.Images;
                    return View(auctionData);
                }
            }
        }

        [HttpGet]
        [Route("delete-auction")]
        public IActionResult DeleteAuction(int id)
        {
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Find Auction by id
            Auction auction = auctionDAO.GetAuctionById(id);

            //check if auction belong to this user
            if (!(auction.UserId == userId))
            {
                TempData["Message"] = "Bạn không thể xóa phiên đấu giá người khác!";
            }
            else
            {
                bool flag = auctionDAO.DeleteAuction(auction);
                TempData["Message"] = flag ? "Xoá đấu giá thành công!" : "Xoá đấu giá thất bại!";
            }

            return Redirect("manage-auction");
        }

        private bool ValidateAuction(AuctionDataModel auctionData)
        {
            var flag = true;

            //validate the end price with start price
            if (auctionData.EndPrice < auctionData.StartPrice)
            {
                ModelState.AddModelError("EndPrice", "Giá kết thúc phải lớn hơn giá khởi điểm");
                flag = false;
            }

            //validate the start time
            if (auctionData.StartTime.CompareTo(DateTime.Now) < 0)
            {
                ModelState.AddModelError("StartTime", "Thời gian bắt đầu phải sau thời điểm hiện tại!");
                flag = false;
            }

            //validate the end time
            if (auctionData.EndTime.CompareTo(auctionData.StartTime) < 0)
            {
                ModelState.AddModelError("EndTime", "Thời gian kết thúc phải sau thời điểm hiện tại!");
                flag = false;
            }

            return flag;
        }

        private bool ValidateAuction(AuctionEditDataModel auctionData)
        {
            var flag = true;

            //validate the end price with start price
            if (auctionData.EndPrice < auctionData.StartPrice)
            {
                ModelState.AddModelError("EndPrice", "Giá kết thúc phải lớn hơn giá khởi điểm");
                flag = false;
            }

            //validate the start time
            if (auctionData.StartTime.CompareTo(DateTime.Now) < 0)
            {
                ModelState.AddModelError("StartTime", "Thời gian bắt đầu phải sau thời điểm hiện tại!");
                flag = false;
            }

            //validate the end time
            if (auctionData.EndTime.CompareTo(auctionData.StartTime) < 0)
            {
                ModelState.AddModelError("EndTime", "Thời gian kết thúc phải sau thời điểm hiện tại!");
                flag = false;
            }

            return flag;
        }
    }
}
