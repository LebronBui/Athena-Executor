using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.Models;

namespace RealEstateAuction.Controllers
{
    public class StaffController : Controller
    {
        PaymentDAO paymentDAO = new PaymentDAO();
        TicketDAO ticketDAO = new TicketDAO();

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
    }
}
