using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.Models;
using System.Security.Claims;

namespace RealEstateAuction.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly NotificationDAO notificationDAO;

        public NotificationViewComponent(NotificationDAO context)
        {
            notificationDAO = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = HttpContext.User;
            var id = Int32.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
            if (id != null)
            {
                var result = notificationDAO.getNotificationByMember(id);
                var notifications = result.Take(5).ToList();
                ViewData["notiNumb"] = notifications.Count;
                return View(notifications);
            }
            else
            {
                return View(new List<Notification>());
            }
        }
    }
}
