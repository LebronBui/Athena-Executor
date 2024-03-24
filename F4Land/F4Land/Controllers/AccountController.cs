using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using RealEstateAuction.Services;
using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;


namespace RealEstateAuction.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserDAO userDAO;
        private readonly AuctionDAO auctionDAO;
        private readonly BankDAO bankDAO;
        private readonly PaymentDAO paymentDAO;
        private readonly TicketDAO ticketDAO;
        private readonly AuctionBiddingDAO auctionBiddingDAO;
        private readonly CategoryDAO categoryDAO;
        private IMapper _mapper;
        private Pagination pagination;
        private NotificationDAO notificationDAO;

        public AccountController(IMapper mapper)
        {
            pagination = new Pagination();
            auctionDAO = new AuctionDAO();
            userDAO = new UserDAO();
            _mapper = mapper;
            bankDAO = new BankDAO();
            paymentDAO = new PaymentDAO();
            ticketDAO = new TicketDAO();
            auctionBiddingDAO = new AuctionBiddingDAO();
            categoryDAO = new CategoryDAO();
            notificationDAO = new NotificationDAO();
        }

        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
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
        [Authorize]
        [Route("change-password")]
        public IActionResult ChangePassword()
        {
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
            //get user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //get wallet of user
            decimal wallet = userDAO.GetUserById(userId).Wallet;

            //check if wallet of user is enough to create auction
            if (wallet < 5000000)
            {
                TempData["Message"] = "Ví của bạn không đủ 5.000.000đ để tạo phiên đấu giá";
                return Redirect("manage-auction");
            }

            //set the fee of create auction
            ViewBag.Fee = DataModel.Constant.Fee;

            return View();
        }

        [HttpPost]
        [Route("create-auction")]
        public IActionResult CreateAuction([FromForm] AuctionDataModel auctionData)
        {
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
                    //get user by id
                    int userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    User user = userDAO.GetUserById(userId);

                    auctionData.UserId = user.Id;
                    auctionData.Status = 1;
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

                        //update wallet of user
                        user.Wallet -= DataModel.Constant.Fee;
                        userDAO.UpdateUser(user);

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
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Find Auction by id
            Auction auction = auctionDAO.GetAuctionById(id);

            if (DateTime.Now.CompareTo(auction.StartTime) > 0)
            {
                TempData["Message"] = "Không thể chỉnh sửa phiên đấu giá đã bắt đầu!";
                return Redirect("manage-auction");
            }

            AuctionEditDataModel auctionData = _mapper.Map<Auction, AuctionEditDataModel>(auction);
            //check if auction belong to this user
            if (!(auction.UserId == userId))
            {
                TempData["Message"] = "Bạn không thể quản lý phiên đấu giá người khác!";
                return Redirect("manage-auction");
            }

            return View(auctionData);
        }

        [HttpGet]
        [Route("my-auction-details")]
        public IActionResult MyAuctionDetails(int id)
        {
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Find Auction by id
            Auction? auction = auctionDAO.GetAuctionById(id);

            //Check if auction is exist
            if (auction == null)
            {
                TempData["Message"] = "Không tìm thấy phiên đấu giá này!";
                return Redirect("manage-auction");
            }

            //check if auction belong to this user
            if (!(auction.UserId == userId))
            {
                TempData["Message"] = "Bạn không thể xem chi tiết phiên đấu giá người khác!";
                return Redirect("manage-auction");
            }

            AuctionEditDataModel auctionData = _mapper.Map<Auction, AuctionEditDataModel>(auction);

            return View(auctionData);
        }

        [HttpPost]
        [Route("edit-auction")]
        public IActionResult EditAuction([FromForm] AuctionEditDataModel auctionData)
        {

            //get auction by id
            Auction auction = auctionDAO.GetApprovedAuction(auctionData.Id.Value);
            if (auction != null)
            {
                if (DateTime.Now.CompareTo(auction.StartTime) > 0)
                {
                    TempData["Message"] = "Không thể chỉnh sửa phiên đấu giá đã bắt đầu!";
                    return Redirect("manage-auction");
                }

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
            TempData["Message"] = "Cập nhật phiên đấu giá thất bại!";

            return Redirect("manage-auction");
        }


        [HttpGet]
        [Route("delete-auction")]
        public IActionResult DeleteAuction(int id)
        {
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Find Auction by id
            Auction auction = auctionDAO.GetApprovedAuction(id);

            //check if auction belong to this user
            if (auction != null)
            {
                if (!(auction.UserId == userId))
                {
                    TempData["Message"] = "Bạn không thể xóa phiên đấu giá người khác!";
                }
                else
                {
                    bool flag = auctionDAO.DeleteAuction(auction);
                    TempData["Message"] = flag ? "Xoá đấu giá thành công!" : "Xoá đấu giá thất bại!";
                }
            }
            else
            {
                TempData["Message"] = "Xoá đấu giá thất bại!";
            }


            return Redirect("manage-auction");
        }

        [HttpGet("join-auction")]
        public IActionResult JoinAuction(int auctionId)
        {
            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //find user by id
            User user = userDAO.GetUserById(userId);

            //Find Auction by id
            Auction auction = auctionDAO.GetAuctionById(auctionId);

            //Check auction is exist
            if (auction == null)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại!";
                return Redirect("manage-auction");
            }

            //check if auction belong to this user
            if (auction.UserId == userId)
            {
                TempData["Message"] = "Bạn không thể tham gia đấu giá phiên đấu giá của mình!";
                return Redirect("/auction-details?auctionId=" + auctionId);
            }

            if (DateTime.Now < auction.StartTime)
            {
                TempData["Message"] = "Phiên đấu giá chưa bắt đầu!";
                return Redirect("/auction-details?auctionId=" + auctionId);
            }

            //check if auction is expired
            if (auction.EndTime.CompareTo(DateTime.Now) < 0 || auction.Status == (int)AuctionStatus.Kết_thúc)
            {
                TempData["Message"] = "Phiên đấu giá đã kết thúc!";
                return Redirect("/auction-details?auctionId=" + auctionId);
            }

            //check if user has joined auction
            if (auctionDAO.IsUserJoinedAuction(user, auctionId))
            {
                TempData["Message"] = "Bạn đã tham gia đấu giá!";
                return Redirect("/auction-details?auctionId=" + auctionId);
            }

            //add new user to list user join auction
            auction.Users.Add(user);

            //update Auction to database
            bool isSuccess = auctionDAO.EditAuction(auction);

            //check if join acution successfull
            if (isSuccess)
            {
                TempData["Message"] = "Tham gia đấu giá thành công!";
            }
            else
            {
                TempData["Message"] = "Tham gia đấu giá thất bại!";
            }

            return Redirect("/auction-details?auctionId=" + auctionId);
        }

        [HttpPost("bidding-auction")]
        [Authorize(Roles = "Member")]
        public IActionResult Bidding(BiddingDataModel biddingDataModel)
        {
            //Get current url
            string url = Request.Headers["Referer"];

            //get user by id
            User user = userDAO.GetUserById(biddingDataModel.MemberId);

            //Get Auction by id
            Auction? auction = auctionDAO.GetAuctionById(biddingDataModel.AuctionId);


            //Check if auction is exist
            if (auction == null)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại!";
                return Redirect(url);
            }

            //Check is user is owner of auction
            if (auction.UserId == user.Id)
            {
                TempData["Message"] = "Bạn không thể đấu giá phiên đấu giá của mình!";
                return Redirect(url);
            }

            //Check if auction is started
            if (auction.StartTime > DateTime.Now)
            {
                TempData["Message"] = "Phiên đấu giá chưa bắt đầu!";
                return Redirect(url);
            }

            //Check if auction is expired
            if (auction.EndTime.CompareTo(DateTime.Now) < 0 || auction.Status == (int)AuctionStatus.Kết_thúc)
            {
                TempData["Message"] = "Phiên đấu giá đã kết thúc!";
                return Redirect(url);
            }

            //Check user has joined auction
            bool isJoined = auctionDAO.IsUserJoinedAuction(user, biddingDataModel.AuctionId);
            if (!isJoined)
            {
                TempData["Message"] = "Bạn chưa tham gia đấu giá!";
                return Redirect(url);
            }

            //Check if bidding price is greater than start price
            if (biddingDataModel.BiddingPrice < auction.StartPrice)
            {
                TempData["Message"] = "Giá đấu phải lớn hơn giá khởi điểm!";
                return Redirect(url);
            }

            //check if bidding price is greater than end price
            if (biddingDataModel.BiddingPrice > auction.EndPrice)
            {
                TempData["Message"] = "Giá đấu phải nhỏ hơn giá kết thúc!";
                return Redirect(url);
            }

            //Get list bidding of auction
            List<AuctionBidding> auctionBiddings = auctionBiddingDAO.GetAuctionBiddings(biddingDataModel.AuctionId);

            //Check bidding price of participant is greater than the max price
            if (auctionBiddings.Count > 0)
            {
                decimal maxPrice = auctionBiddings.Max(ab => ab.BiddingPrice);
                if (biddingDataModel.BiddingPrice <= maxPrice)
                {
                    TempData["Message"] = "Giá đấu phải lớn hơn giá đấu cao nhất hiện tại!";
                    return Redirect(url);
                }
            }

            //Get the last bidding of user for this auction
            AuctionBidding? lastBidding = auctionBiddingDAO.GetLastBiddingByUser(biddingDataModel.AuctionId, biddingDataModel.MemberId);

            //Get the last bidding of user for this auction if exist
            if (lastBidding != null)
            {
                //Get the last bidding price of user for this auction
                decimal lastBiddingPrice = lastBidding.BiddingPrice;

                //Refund the price of bidding to user
                user.Wallet += lastBiddingPrice;
            }

            //Minus the price of bidding from user wallet
            user.Wallet -= biddingDataModel.BiddingPrice;

            //Check if user wallet is enough to bidding
            if (user.Wallet < 0)
            {
                TempData["Message"] = "Ví của bạn không đủ để đấu giá!";
                return Redirect(url);
            }

            //If bidding price is equal to end price
            if (biddingDataModel.BiddingPrice == auction.EndPrice)
            {
                //Update status of auction to end
                auction.Status = (int)AuctionStatus.Kết_thúc;

                //keep 10% of the price as a deposit
                var deposit = biddingDataModel.BiddingPrice * 0.1m;

                //return the rest of the price to the winner
                user.Wallet += biddingDataModel.BiddingPrice - deposit;

                //Update Auction to database
                bool isSuccess = auctionDAO.EditAuction(auction);
            }

            //Update wallet of user
            bool updateWallet = userDAO.UpdateUser(user);
            //Check if update wallet successfull
            if (!updateWallet)
            {
                TempData["Message"] = "Có lỗi khi xử lý ví!";
                return Redirect(url);
            }
            else
            {
                //If bidding price is valid add to database
                //Map to AuctionBidding model
                AuctionBidding auctionBidding = _mapper.Map<BiddingDataModel, AuctionBidding>(biddingDataModel);
                auctionBidding.TimeBidding = DateTime.Now;

                bool isSuccess = auctionBiddingDAO.AddAuctionBidding(auctionBidding);
                Auction auctionEnd = auctionDAO.GetAuctionEndById(biddingDataModel.AuctionId);
                if (auctionEnd != null && auctionEnd.Status == (int)AuctionStatus.Kết_thúc)
                {
                    var winnerId = auctionDAO.GetWinnerId(auction);
                    var notifications = new List<Notification>();

                    foreach (var bidding in auction.AuctionBiddings)
                    {
                        var notification = new Notification();
                        if (bidding.MemberId == winnerId)
                        {
                            notification = new Notification
                            {
                                Description = $"Bạn đã thắng phiên đấu giá {auction.Title}, 10% tiền đấu giá sẽ bị giữ lại làm cọc!",
                                ToUser = bidding.MemberId,
                                Link = $"/auction-details?auctionId={auction.Id}",
                                IsRead = false,
                            };
                        }
                        else
                        {
                            var userEnd = userDAO.GetUserById(bidding.MemberId);

                            userEnd.Wallet += bidding.BiddingPrice;

                            userDAO.UpdateUser(userEnd);
                            notification = new Notification
                            {
                                Description = $"Phiên đấu giá {auction.Title} đã kết thúc",
                                ToUser = bidding.MemberId,
                                Link = $"/auction-details?auctionId={auction.Id}",
                                IsRead = false,
                            };
                        }
                        notifications.Add(notification);
                    }
                    notificationDAO.insertList(notifications);
                }

                //Check user bidding successfull
                if (isSuccess)
                {
                    TempData["Message"] = "Đấu giá thành công!";
                }
                else
                {
                    TempData["Message"] = "Có lỗi xảy ra khi đặt giá!";
                }
            }

            return Redirect(url);
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

        [HttpGet("/top-up")]
        [Authorize(Roles = "Member")]
        public IActionResult TopUp(int? page)
        {
            int PageNumber = (page ?? 1);
            ViewData["List"] = paymentDAO.listByUserId(Int32.Parse(User.FindFirstValue("Id")), PageNumber);

            ViewData["Banks"] = bankDAO.listBankings();

            return View();
        }

        [HttpPost("/top-up-post")]
        [Authorize(Roles = "Member")]
        public IActionResult TopUpPost([FromForm] PaymentDataModel paymentData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Payment payment;
                    switch (paymentData.Action)
                    {
                        case PaymentType.TopUp:
                            payment = new Payment()
                            {
                                BankId = paymentData.BankId,
                                Amount = paymentData.Amount,
                                UserBankName = paymentData.UserBankName,
                                UserBankAccount = paymentData.UserAccountNumber,
                                Code = $"NAP_{DateTime.Now.ToShortTimeString()}",
                                TransactionDate = DateTime.Now,
                                Status = (int)PaymentStatus.Pending,
                                UserId = Int32.Parse(User.FindFirstValue("Id")),
                                Type = (byte)paymentData.Action,
                            };
                            paymentDAO.insert(payment);
                            payment.Bank = bankDAO.bankDetail(paymentData.BankId);

                            break;
                        case PaymentType.Withdraw:
                            var user = userDAO.GetUserById(Int32.Parse(User.FindFirstValue("Id")));
                            Console.Write(user.Id);
                            if (user.Wallet < paymentData.Amount)
                            {
                                TempData["Message"] = "Không thể tạo yêu cầu do số tiền rút cao hơn số tiền trong ví";

                                return RedirectToAction("TopUp");
                            }
                            payment = new Payment()
                            {
                                Amount = paymentData.Amount,
                                UserBankAccount = paymentData.UserAccountNumber,
                                UserBankName = paymentData.UserBankName,
                                Code = $"RUT_{DateTime.Now.ToShortTimeString()}",
                                TransactionDate = DateTime.Now,
                                Status = (int)PaymentStatus.Pending,
                                UserId = Int32.Parse(User.FindFirstValue("Id")),
                                Type = (byte)paymentData.Action,
                            };
                            paymentDAO.insert(payment);

                            break;
                        case PaymentType.Refund:
                            int paymentId = Int32.Parse(Request.Form["PaymentId"]);
                            var paymentRefund = paymentDAO.getPaymentRefund(paymentId);
                            var newPayment = new Payment()
                            {
                                Amount = paymentRefund.Amount,
                                UserBankAccount = paymentRefund.UserBankAccount,
                                UserBankName = paymentRefund.UserBankName,
                                Code = $"HOAN_{DateTime.Now.ToShortTimeString()}",
                                TransactionDate = DateTime.Now,
                                Status = (int)PaymentStatus.Pending,
                                UserId = Int32.Parse(User.FindFirstValue("Id")),
                                Type = (byte)paymentData.Action,
                            };
                            paymentDAO.insert(newPayment);
                            break;
                    }
                    TempData["Message"] = "Tạo yêu cầu thành công";
                }
                else
                {
                    Console.WriteLine(1);
                    foreach (var modelStateEntry in ModelState.Values)
                    {
                        foreach (var error in modelStateEntry.Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }
                    TempData["Message"] = "Tạo yêu cầu thất bại";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(2);
                Console.WriteLine(ex.Message);

                TempData["Message"] = "Tạo yêu cầu thất bại";
            }

            return RedirectToAction("TopUp");
        }

        [Authorize(Roles = "Member")]
        [HttpGet("member/list-ticket")]
        public IActionResult ListTicketUser(int? page)
        {
            int PageNumber = (page ?? 1);
            var list = ticketDAO.listTicketByUser(Int32.Parse(User.FindFirstValue("Id")), PageNumber);
            if (list.PageCount != 0 && list.PageCount < PageNumber)
            {
                TempData["Message"] = "Sô trang không hợp lệ";
                return Redirect("member/list-ticket");
            }
            ViewData["List"] = list;
            return View();
        }

        [Authorize(Roles = "Member")]
        [HttpGet("/member/list-ticket/{id}")]
        public IActionResult TicketDetailUser(int id)
        {
            var ticket = ticketDAO.ticketDetail(id);
            if (ticket == null || ticket.UserId != Int32.Parse(User.FindFirstValue("Id")))
            {
                TempData["Message"] = "Yêu cầu hỗ trợ không tồn tại";
                return RedirectToAction("ListTicket");
            }
            ViewData["Ticket"] = ticket;
            ViewData["IdUser"] = User.FindFirstValue("Id");
            return View();
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        [Route("member/reply")]
        public IActionResult ReplyUser([FromForm] TicketCommentDataModel commentData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Vui lòng kiểm tra lại thông tin";
                    return RedirectToAction("TicketDetail", "Account", new { Id = commentData.TicketId });
                }
                var ticket = ticketDAO.ticketDetail(commentData.TicketId);
                var idUser = Int32.Parse(User.FindFirstValue("Id"));
                if (ticket == null || ticket.UserId != idUser)
                {
                    TempData["Message"] = "Yêu cầu hỗ trợ không tồn tại";
                    return RedirectToAction("listTicket");
                }
                else
                {
                    TicketComment commentInsert = new TicketComment
                    {
                        UserId = idUser,
                        Comment = commentData.Comment,
                        TicketId = commentData.TicketId,
                    };
                    ticketDAO.insertComment(commentInsert);
                    TempData["Message"] = "Trả lời thành công";
                }
                return RedirectToAction("TicketDetailUser", "Account", new { Id = commentData.TicketId });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi hệ thống, xin vui lòng thử lại";
                return RedirectToAction("ListTicket");
            }
        }

        [HttpPost("/create-ticket")]
        [Authorize(Roles = "Member")]
        public IActionResult CreateTicket([FromForm] TicketDataModel ticketData)
        {
            if (ModelState.IsValid)
            {
                Ticket ticket = new Ticket()
                {
                    UserId = Int32.Parse(User.FindFirstValue("Id")),
                    Title = ticketData.Title,
                    Description = ticketData.Description,
                    Status = (byte)TicketStatus.Opening,
                };
                if (ticketData.ImageFiles != null)
                {
                    List<TicketImage> images = new List<TicketImage>();
                    foreach (var file in ticketData.ImageFiles)
                    {
                        var pathImage = FileUpload.UploadImageProduct(file);
                        if (pathImage != null)
                        {
                            TicketImage image = new TicketImage();
                            image.Url = pathImage;
                            images.Add(image);
                        }
                    }
                    ticket.TicketImages = images;

                }

                ticketDAO.createTicket(ticket);
                TempData["Message"] = "Tạo yêu cầu thành công";
            }
            else
            {
                TempData["Message"] = "Tạo yêu cầu thất bại";
            }

            return RedirectToAction("ListTicketUser");
        }

        [HttpGet("list-participant")]
        [Authorize(Roles = "Member")]
        public IActionResult ListParticipant(int? auctionId, int? pageNumber)
        {
            ViewBag.isFinished = false;

            //Check if auctionId is null
            if (!auctionId.HasValue)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại";
                return RedirectToAction("ManageAuction");
            }

            //Get auction by id
            var auction = auctionDAO.GetAuctionById(auctionId.Value);

            if (auction == null)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại";
                return RedirectToAction("ManageAuction");
            }

            //Get current userId
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Check if auction belong to this user
            if (auction.UserId != userId)
            {
                TempData["Message"] = "Bạn không thể xem danh sách người tham gia của phiên đấu giá này!";
                return RedirectToAction("ManageAuction");
            }

            //Check auction is finished
            if (auction.Status == (int)AuctionStatus.Kết_thúc)
            {
                ViewBag.isFinished = true;
            }


            if (pageNumber.HasValue)
            {
                pagination.PageNumber = pageNumber.Value;
            }

            //Get list of participant
            var list = auctionBiddingDAO.GetParticipantByAuctionId(auctionId.Value, pagination);

            //Count number of participant
            int participantCount = auctionBiddingDAO.CountParticipant(auctionId.Value);

            //Get number of page
            int pageSize = (int)Math.Ceiling((double)participantCount / pagination.RecordPerPage);

            ViewBag.currentPage = pagination.PageNumber;
            ViewBag.pageSize = pageSize;

            return View(list);
        }

        [HttpGet("list-joining")]
        [Authorize(Roles = "Member")]
        public IActionResult ListJoining(int? auctionId, int? pageNumber)
        {
            //Check if auctionId is null
            if (!auctionId.HasValue)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại";
                return RedirectToAction("ManageAuction");
            }

            //Get auction by id
            var auction = auctionDAO.GetAuctionById(auctionId.Value);

            if (auction == null)
            {
                TempData["Message"] = "Phiên đấu giá không tồn tại";
                return RedirectToAction("ManageAuction");
            }

            //Get current userId
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //Check if auction belong to this user
            if (auction.UserId != userId)
            {
                TempData["Message"] = "Bạn không thể xem danh sách người tham gia của phiên đấu giá này!";
                return RedirectToAction("ManageAuction");
            }

            if (pageNumber.HasValue)
            {
                pagination.PageNumber = pageNumber.Value;
            }

            //Get list of participant
            var list = auctionBiddingDAO.GetJoiningByAuctionId(auctionId.Value, pagination);

            //Count number of participant
            int participantCount = auctionBiddingDAO.CountJoining(auctionId.Value);

            //Get number of page
            int pageSize = (int)Math.Ceiling((double)participantCount / pagination.RecordPerPage);

            ViewBag.currentPage = pagination.PageNumber;
            ViewBag.pageSize = pageSize;

            return View(list);
        }

        [HttpGet("auction-histories")]
        [Authorize(Roles = "Member")]
        public IActionResult AuctionHistory(int? pageNumber)
        {
            int page = pageNumber ?? 1;
            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var list = auctionDAO.GetAuctionHistoryByUser(userId, page);
            List<AuctionBidding> winners = new List<AuctionBidding>();
            foreach (var i in list)
            {
                winners.Add(auctionDAO.GetMaxBiddingForAuction(i.Id));
            }

            // Tạo danh sách kết hợp từ list và winners
            var combinedList = new List<dynamic>();
            foreach (var auction in list)
            {
                var winner = new AuctionBidding();
                try
                {
                    winner = winners.SingleOrDefault(w => w.AuctionId == auction.Id && w.MemberId == userId);
                }
                catch
                {
                    winner = null;
                }

                combinedList.Add(new { Auction = auction, Winner = winner });
            }

            ViewData["AuctionList"] = list;

            return View(combinedList);
        }
    }
}
