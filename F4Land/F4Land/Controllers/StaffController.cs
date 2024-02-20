using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;
using System.Security.Claims;

namespace RealEstateAuction.Controllers
{
    public class StaffController : Controller
    {
        PaymentDAO paymentDAO = new PaymentDAO();
        TicketDAO ticketDAO = new TicketDAO();
        private Pagination pagination;
        private readonly AuctionDAO auctionDAO;

        public StaffController()
        {
            auctionDAO = new AuctionDAO();
            pagination = new Pagination();
        }

        [Authorize(Roles = "Staff")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("staff/payments")]
        public IActionResult listPayment(int? page)
        {
            int PageNumber = (page ?? 1);
            var list = paymentDAO.list(PageNumber);
            return View(list);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("staff/tickets")]
        public IActionResult listTicket(int? page)
        {
            int PageNumber = (page ?? 1);
            var list = ticketDAO.listTicket(PageNumber);
            return View(list);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("staff/tickets/{id}")]
        public IActionResult ticketDetail(int id)
        {
            var ticket = ticketDAO.ticketDetail(id);
            if (ticket == null)
            {
                TempData["Message"] = "Ticket Not Found";
                return RedirectToAction("listTicket");
            }
            return View(ticket);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        [Route("payments/{id}/reply")]
        public IActionResult reply(Ticket ticket, int id)
        {
            return View();
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
            Auction auction = auctionDAO.GetAuctionById(auctionId);

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
            Auction auction = auctionDAO.GetAuctionById(auctionId);

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
