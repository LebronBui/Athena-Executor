using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace RealEstateAuction.Controllers
{
    public class StaffController : Controller
    {
        private readonly PaymentDAO paymentDAO;
        private readonly TicketDAO ticketDAO;
        private readonly UserDAO userDAO;
        private Pagination pagination;
        private readonly AuctionDAO auctionDAO;
        private readonly NotificationDAO notificationDAO;
        private readonly CategoryDAO categoryDAO;

        public StaffController()
        {
            auctionDAO = new AuctionDAO();
            pagination = new Pagination();
            userDAO = new UserDAO();
            ticketDAO = new TicketDAO();
            paymentDAO = new PaymentDAO();
            notificationDAO = new NotificationDAO();
            categoryDAO = new CategoryDAO();
        }

        [Authorize(Roles = "Staff")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("/list-payment")]
        public IActionResult ListPayment(int? page)
        {
            int PageNumber = (page ?? 1);
            var list = paymentDAO.list(PageNumber);
            return View(list);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("/list-payment/update")]
        public IActionResult UpdatePayment()
        {
            try
            {
                var payment = paymentDAO.getPayment(Int32.Parse(Request.Form["id"].ToString()));

                if (payment != null && payment.Status == (int)PaymentStatus.Pending)
                {
                    payment.Status = Byte.Parse(Request.Form["status"].ToString());
                    paymentDAO.topUp(payment, (int)payment.UserId, Int32.Parse(User.FindFirstValue("Id")));
                    if (payment.Status == (int)PaymentStatus.Reject)
                    {
                        notificationDAO.insert(new Notification()
                        {
                            Description = $"Yêu cầu thanh toán bị từ chối",
                            ToUser = payment.UserId,
                            Link = $"/top-up",
                            IsRead = false,
                        });
                    }
                    else
                    {
                        switch (payment.Type)
                        {
                            case (int)PaymentType.TopUp:
                                notificationDAO.insert(new Notification()
                                {
                                    Description = $"Nạp thành công {payment.Amount} vào ví",
                                    ToUser = payment.UserId,
                                    Link = $"/top-up",
                                    IsRead = false,
                                });
                                break;
                            case (int)PaymentType.Withdraw:
                                notificationDAO.insert(new Notification()
                                {
                                    Description = $"Rút thành công {payment.Amount} khỏi ví",
                                    ToUser = payment.UserId,
                                    Link = $"/top-up",
                                    IsRead = false,
                                });
                                break;
                            case (int)PaymentType.Refund:
                                notificationDAO.insert(new Notification()
                                {
                                    Description = $"Hoàn tiền thành công",
                                    ToUser = payment.UserId,
                                    Link = $"/top-up",
                                    IsRead = false,
                                });
                                break;
                        }
                    }
                    TempData["Message"] = "Cập nhật thành công";
                }
                else
                {
                    TempData["Message"] = "Thanh toán không tồn tại";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Message"] = "Lỗi hệ thống, vui lòng thử lại";
            }

            return RedirectToAction("ListPayment");
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("/list-ticket")]
        public IActionResult ListTicket(int? page)
        {
            int PageNumber = (page ?? 1);
            var list = ticketDAO.listTicketByStaff(Int32.Parse(User.FindFirstValue("Id")), PageNumber);
            if (list.PageCount != 0 && list.PageCount < PageNumber)
            {
                TempData["Message"] = "Sô trang không hợp lệ";
                return Redirect("/list-ticket");
            }
            return View(list);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("/list-ticket/update")]
        public IActionResult UpdateTicket()
        {
            try
            {
                var ticket = ticketDAO.ticketDetail(Int32.Parse(Request.Form["id"].ToString()));
                if (ticket != null
                    && ticket.Status == (int)TicketStatus.Opening
                    && ticket.StaffId == Int32.Parse(User.FindFirstValue("Id"))
                    )
                {
                    ticket.Status = Byte.Parse(Request.Form["status"].ToString());
                    ticketDAO.update(ticket);
                    TempData["Message"] = "Cập nhật thành công";
                }
                else
                {
                    TempData["Message"] = "Yêu cầu hỗ trợ không tồn tại";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi hệ thống, vui lòng thử lại";
            }

            return RedirectToAction("listTicket");
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("list-ticket/{id}")]
        public IActionResult TicketDetail(int id)
        {
            var ticket = ticketDAO.ticketDetail(id);
            if (ticket == null || ticket.StaffId != Int32.Parse(User.FindFirstValue("Id")))
            {
                TempData["Message"] = "Yêu cầu hỗ trợ không tồn tại";
                return RedirectToAction("listTicket");
            }
            ViewData["Ticket"] = ticket;
            ViewData["IdStaff"] = User.FindFirstValue("Id");
            return View();
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        [Route("staff/reply")]
        public IActionResult reply([FromForm] TicketCommentDataModel commentData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Vui lòng kiểm tra lại thông tin";
                    return RedirectToAction("TicketDetail", "Staff", new { Id = commentData.TicketId });
                }
                var ticket = ticketDAO.ticketDetail(commentData.TicketId);
                var idStaff = Int32.Parse(User.FindFirstValue("Id"));
                if (ticket == null || ticket.StaffId != idStaff)
                {
                    TempData["Message"] = "Yêu cầu hỗ trợ không tồn tại";
                    return RedirectToAction("listTicket");
                }
                else
                {
                    TicketComment commentInsert = new TicketComment
                    {
                        UserId = idStaff,
                        Comment = commentData.Comment,
                        TicketId = commentData.TicketId,
                    };
                    ticketDAO.insertComment(commentInsert);
                    TempData["Message"] = "Trả lời thành công";
                    if (commentData.IsClosed == true)
                    {
                        ticket.Status = (byte)TicketStatus.Closed;
                        ticketDAO.update(ticket);
                        TempData["Message"] = "Đóng yêu cầu hỗ trợ thành công";
                    }
                    notificationDAO.insert(new Notification()
                    {
                        Description = $"Yêu cầu hỗ trợ đã được trả lời",
                        ToUser = ticket.UserId,
                        Link = $"/member/list-ticket/{ticket.Id}",
                        IsRead = false,
                    });
                }
                return RedirectToAction("TicketDetail", "Staff", new { Id = commentData.TicketId });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi hệ thống, xin vui lòng thử lại";
                return RedirectToAction("listTicket");
            }

        }

        [Authorize(Roles = "Staff")]
        [HttpGet]
        [Route("list-auction-staff")]
        public IActionResult ListAuction(int? pageNumber)
        {
            ViewData["categories"] = categoryDAO.GetCategories();
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

            //get auction by staff id
            List<Auction> auctions = auctionDAO.GetAuctionByStaffId(userId, pagination);

            int auctionCount = auctionDAO.CountAuctionByStaffId(userId);
            int pageSize = (int)Math.Ceiling((double)auctionCount / pagination.RecordPerPage);

            ViewBag.currentPage = pagination.PageNumber;
            ViewBag.pageSize = pageSize;

            return View(auctions);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet]
        [Route("details-auction")]
        public IActionResult DetailsAuction(int auctionId)
        {
            ViewData["categories"] = categoryDAO.GetCategories();
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }

            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //get auction by Id
            Auction auction = auctionDAO.GetAuctionStaffById(auctionId);

            //check if staff manage this auction or not
            if (auction.ApproverId == userId)
            {
                return View(auction);
            }
            else
            {
                TempData["Message"] = "Bạn không thể quản lý phiên đấu giá này";
                return Redirect("list-auction-staff");
            }
        }

        [Authorize(Roles = "Staff")]
        [HttpGet]
        [Route("approve-auction")]
        public IActionResult ListAuction(int auctionId, int status)
        {
            ViewData["categories"] = categoryDAO.GetCategories();
            //check user login or not
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Message"] = "Vui lòng đăng nhập để quản lý đấu giá!";
                return RedirectToAction("Index", "Home");
            }

            //get current user id
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            //get auction by Id
            Auction auction = auctionDAO.GetAuctionStaffById(auctionId);

            //check if staff manage this auction or not
            if (auction.ApproverId == userId)
            {
                auction.Status = byte.Parse(status.ToString());

                //check if auction is rejected
                if (status == (int)AuctionStatus.Từ_chối)
                {
                    auction.Reason = Request.Query["reason"];
                    //get owner auction id
                    var ownerAuction = auctionDAO.GetAuctionById(auctionId);

                    //get owner
                    var owner = userDAO.GetUserById(ownerAuction.User.Id);
                    owner.Wallet += Constant.Fee;

                    //update user
                    var isSuccess = userDAO.UpdateUser(owner);
                    //check if update user wallet success
                    if (!isSuccess)
                    {
                        TempData["Message"] = "Đã có lỗi xảy ra khi cập nhật ví người đăng đấu giá!";
                        return Redirect("list-auction-staff");
                    }
                    notificationDAO.insert(new Notification()
                    {
                        Description = $"Phiên đấu giá {auction.Title} bị từ chối",
                        ToUser = auction.User.Id,
                        Link = $"/manage-auction",
                        IsRead = false,
                    });
                }
                else
                {
                    auction.Categories.Add(categoryDAO.GetCategoryById(Int32.Parse(Request.Query["categoryId"])));
                    auction.Approver.Wallet += Constant.Fee;
                }


                bool flag = auctionDAO.EditAuction(auction);
                if (flag)
                {
                    TempData["Message"] = "Phê duyệt thành công!";
                    notificationDAO.insert(new Notification()
                    {
                        Description = $"Phiên đấu giá {auction.Title} đã được phê duyệt",
                        ToUser = auction.User.Id,
                        Link = $"/manage-auction",
                        IsRead = false,
                    });
                }
                else
                {
                    TempData["Message"] = "Phê duyệt thất bại";
                }
            }
            else
            {
                TempData["Message"] = "Bạn không thể quản lý phiên đấu giá này";
            }

            return Redirect("list-auction-staff");
        }
        [Authorize(Roles = "Staff")]
        [HttpGet("confirm-auction")]
        public IActionResult ConfirmAuction(int auctionId, int status)
        {

            //get auction by Id
            Auction auction = auctionDAO.GetAuctionById(auctionId);

            //check status
            if (status == 5)
            {
                //get the price of winner
                var price = auctionDAO.GetMaxPrice(auctionId);

                var winnerId = auctionDAO.GetWinnerId(auction);

                //get the winner
                var winner = userDAO.GetUserById(winnerId);

                //return the 10% of winner price fee
                winner.Wallet += Math.Round(price * 0.1m);

                //update user
                userDAO.UpdateUser(winner);

                //update status of auction
                auction.Status = byte.Parse(status.ToString());

                //update auction
                bool flag = auctionDAO.EditAuction(auction);

                //check if update auction success
                if (flag)
                {
                    TempData["Message"] = "Cập nhật thành công!";
                }
                else
                {
                    TempData["Message"] = "Cập nhật thất bại";
                }
            }
            else
            {
                //get the price of winner
                var price = auctionDAO.GetMaxPrice(auctionId);

                //take 10% of the price as a deposit
                var deposit = Math.Round(price * 0.1m);

                //keep 5% of deposit for the system
                var systemFee = Math.Round(deposit * 0.05m);
                Console.WriteLine(price + " " + deposit);
                //get the winner id
                var winnerId = auctionDAO.GetWinnerId(auction);

                //get the winner
                var winner = userDAO.GetUserById(winnerId);

                //return the rest of the deposit to the winner
                winner.Wallet += deposit - systemFee;

                //update user
                userDAO.UpdateUser(winner);

                //update status of auction
                auction.Status = byte.Parse(status.ToString());

                //update auction
                bool flag = auctionDAO.EditAuction(auction);
                if (flag)
                {
                    TempData["Message"] = "Cập nhật thành công!";
                }
                else
                {
                    TempData["Message"] = "Cập nhật thất bại";
                }
            }

            return Redirect("list-auction-staff");
        }
    }
}
