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
                .ToPagedList(page, 10);
        }

        public bool createTicket(Ticket ticket)
        {
            try
            {
                context.Tickets.Add(ticket);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public Ticket ticketDetail(int id)
        {
            return context.Tickets.SingleOrDefault(e => e.Id == id);
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
