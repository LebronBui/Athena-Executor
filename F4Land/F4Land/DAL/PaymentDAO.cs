using X.PagedList;
using RealEstateAuction.Models;
using Microsoft.EntityFrameworkCore;

namespace RealEstateAuction.DAL
{
    public class PaymentDAO
    {
        private readonly RealEstateContext context;

        public PaymentDAO()
        {
            context = new RealEstateContext();
        }

        public bool update(Payment payment)
        {
            try
            {
                context.Payments.Update(payment);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IPagedList<Payment> list(int PageNumber)
        {
            return context.Payments.Include(x => x.User)
                .Include(x => x.Bank)
                .ToPagedList(PageNumber, 10);
        }

        public Payment paymentDetail(int id)
        {
            return context.Payments.Include(x => x.User)
                .Include(x => x.Bank)
                .SingleOrDefault(p => p.Id == id);
        }

        public Payment getPayment(int id)
        {
            return context.Payments.SingleOrDefault(p => p.Id == id);
        }
    }
}
