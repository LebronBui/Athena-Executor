using X.PagedList;
using RealEstateAuction.Models;
using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace RealEstateAuction.DAL
{
    public class TicketDAO
    {
        private readonly RealEstateContext context;

        public TicketDAO()
        {
            context = new RealEstateContext();
        }

        public IPagedList<Ticket> listTicket(int page)
        {
            return context.Tickets.Include(t => t.User)
                .Include(t => t.Staff)
                .ToPagedList(page, 10);
        }

        public IPagedList<Ticket> listTicketByStaff(int staffId, int page)
        {
            return context.Tickets.Include(t => t.User)
                .Where(t => t.StaffId == staffId)
                .ToPagedList(page, 10);
        }

        public IPagedList<Ticket> listTicketByUser(int userId, int page)
        {
            return context.Tickets.Include(t => t.User)
                .Where(t => t.UserId == userId)
                .ToPagedList(page, 10);
        }

        public int createTicket(Ticket ticket)
        {
            try
            {
                context.Tickets.Add(ticket);
                context.SaveChanges();
                return ticket.Id;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public Ticket ticketDetail(int id)
        {
            return context.Tickets.Include(t => t.User)
                .Include(t => t.Staff)
                .Include(t => t.TicketComments)
                .Include(t => t.TicketImages)
                .SingleOrDefault(e => e.Id == id);
        }

        public bool update(Ticket ticket)
        {
            try
            {
                context.Tickets.Update(ticket);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool insertComment(TicketComment ticketComment)
        {
            try
            {
                context.TicketComments.Add(ticketComment);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
