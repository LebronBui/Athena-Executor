using RealEstateAuction.Models;
using X.PagedList;

namespace RealEstateAuction.DAL
{
    public class NotificationDAO
    {
        private readonly RealEstateContext context;
        public NotificationDAO()
        {
            context = new RealEstateContext();
        }

        public IOrderedQueryable<Notification> getNotificationByMember(int id)
        {
            return context.Notifications.Where(x => x.ToUser == id).OrderByDescending(x => x.Id);
        }

        public IPagedList<Notification> listNotificationByMember(int id, int? page)
        {
            int PageNumber = page ?? 1;
            return context.Notifications.Where(x => x.ToUser == id).OrderBy(x => x.Id).ToPagedList(PageNumber, 10);
        }

        public bool insert(Notification notification)
        {
            try
            {
                context.Notifications.Add(notification);
                context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool insertList(List<Notification> notifications)
        {
            try
            {
                context.Notifications.AddRange(notifications);
                context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
