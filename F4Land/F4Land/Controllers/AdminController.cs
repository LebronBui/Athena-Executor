using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;

namespace RealEstateAuction.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserDAO userDAO;
        private readonly BankDAO bankDAO;
        private readonly TicketDAO ticketDAO;

        public AdminController(IMapper mapper)
        {
            userDAO = new UserDAO();
            bankDAO = new BankDAO();
            ticketDAO = new TicketDAO();
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("manage-ticket")]
        public IActionResult ManageTicket(int? page)
        {
            int PageNumber = page ?? 1;
            ViewData["List"] = ticketDAO.listTicket(PageNumber);

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("manage-ticket/{id}")]
        public IActionResult TicketDetailAdmin(int id)
        {
            ViewData["Ticket"] = ticketDAO.ticketDetail(id);
            ViewData["Staffs"] = userDAO.GetStaff();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-ticket")]
        public IActionResult AssignTicket([FromForm] AssignTicketDataModel assignTicketData)
        {
            if (ModelState.IsValid)
            {
                var ticket = ticketDAO.ticketDetail(int.Parse(assignTicketData.TicketId.ToString()));
                ticket.StaffId = int.Parse(assignTicketData.StaffId.ToString());
                ticketDAO.update(ticket);
                TempData["Message"] = "Bàn giao yêu cầu thành công";
            }
            else
            {
                TempData["Message"] = "Bàn giao yêu cầu thất bại";
            }
            return RedirectToAction("ManageTicket");
        }
    }
}
