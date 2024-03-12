using X.PagedList;
using RealEstateAuction.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
using RealEstateAuction.Enums;

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

        public bool topUp(Payment payment, int UserId, int StaffId)
        {
            try
            {
                var user = context.Users.SingleOrDefault(x => x.Id == UserId);
                var staff = context.Users.SingleOrDefault(x => x.Id == StaffId);
                var type = payment.Type;
                if (payment.Status != (int)PaymentStatus.Reject)
                {
                    switch (type)
                    {
                        case (int)PaymentType.TopUp:
                            user.Wallet += (decimal)payment.Amount;
                            staff.Wallet += (decimal)payment.Amount;

                            break;
                        case (int)PaymentType.Withdraw:
                            user.Wallet -= (decimal)payment.Amount;

                            break;
                        case (int)PaymentType.Refund:
                            user.Wallet += (decimal)payment.Amount;
                            staff.Wallet -= (decimal)payment.Amount;

                            break;
                    }
                    List<User> users = new List<User>();
                    users.Add(user);
                    users.Add(staff);
                    context.Users.UpdateRange(users);
                }

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
                .OrderByDescending(x => x.Id)
                .ToPagedList(PageNumber, 10);
        }

        public IPagedList<Payment> listByUserId(int UserId, int PageNumber)
        {
            return context.Payments.Include(x => x.User)
                .Include(x => x.Bank)
                .Where(x => x.User.Id == UserId)
                .OrderByDescending(x => x.Id)
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

        public Payment getPaymentRefund(int id)
        {
            return context.Payments.Where(x => x.Status == (int)PaymentStatus.Approve && x.Type == (int)PaymentType.TopUp).SingleOrDefault(p => p.Id == id);
        }

        public bool insert(Payment payment)
        {
            try
            {
                context.Payments.Add(payment);
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
