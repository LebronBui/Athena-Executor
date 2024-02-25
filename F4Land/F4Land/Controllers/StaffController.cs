using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
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

        public StaffController()
        {
            auctionDAO = new AuctionDAO();
            pagination = new Pagination();
            userDAO = new UserDAO();
            ticketDAO = new TicketDAO();
            paymentDAO = new PaymentDAO();
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
                    paymentDAO.topUp(payment, (int)payment.UserId);
                    TempData["Message"] = "Cập nhật thành công";
                }
                else
                {
                    TempData["Message"] = "Thanh toán không tồn tại";
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
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
                auction.Status = Byte.Parse(status.ToString());
                bool flag = auctionDAO.EditAuction(auction);
                if (flag)
                {
                    TempData["Message"] = "Phê duyệt thành công!";
                }
                else
                {
                    TempData["Message"] = "Phê duyệt thất b";
                }
            }
            else
            {
                TempData["Message"] = "Bạn không thể quản lý phiên đấu giá này";
            }

            return Redirect("list-auction-staff");
        }
    }
}
