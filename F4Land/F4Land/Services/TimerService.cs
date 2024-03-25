using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using RealEstateAuction.DAL;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace RealEstateAuction.Services
{
    public class TimerService
    {
        private readonly ILogger _logger;
        private readonly AuctionDAO _auctionDAO;
        private readonly UserDAO _userDAO;
        private readonly NotificationDAO _notificationDAO;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public TimerService(ILogger logger, IUrlHelperFactory urlHelperFactory)
        {
            _logger = logger;
            _auctionDAO = new AuctionDAO();
            _userDAO = new UserDAO();
            _notificationDAO = new NotificationDAO();
            _urlHelperFactory = urlHelperFactory;
        }

        //Shedules a task to be executed at a specific time
        public void EndAuction(List<Auction> ending)
        {
            if (!ending.Any()) // Check if the list is empty
            {
                return; // If the list is empty, return without doing anything
            }

            // Loop through the list of auctions
            foreach (var auction in ending)
            {
                // Calculate the time left until the auction ends
                TimeSpan ts = auction.EndTime - DateTime.Now;
                // Initialize the timer
                Timer timer = InitializeTimer(auction.Id, ts);
            }
        }

        private void DoEndAuction(int auctionId, Timer timer)
        {
            timer.Dispose();

            //get auction by id
            Auction auction = _auctionDAO.GetAuctionEndById(auctionId);
            //update status of auction to ended
            auction.Status = (int)AuctionStatus.Kết_thúc;
            //update auction to database
            _auctionDAO.EditAuction(auction);
            var notifications = new List<Notification>();
            var notificationForUser = new Notification();

            //get the price of the winner
            var winnerPrice = auction.AuctionBiddings.Max(x => x.BiddingPrice);
            //keep 10% of the price as a deposit
            var deposit = Math.Round(winnerPrice * 0.1m);
            //return the rest of the price to the winner              

            Console.WriteLine($"bidding count: {auction.AuctionBiddings.Count()}");
            foreach (var bidding in auction.AuctionBiddings)
            {
                Console.WriteLine($"bidding price: {bidding.BiddingPrice}, name: {bidding.Member.FullName}");
                var notification = new Notification();
                if (bidding.BiddingPrice == winnerPrice)
                {
                    bidding.Member.Wallet += winnerPrice - deposit;
                    notification = new Notification
                    {
                        Description = $"Bạn đã thắng phiên đấu giá {auction.Title}, 10% tiền đấu giá sẽ bị giữ lại làm cọc!",
                        ToUser = bidding.MemberId,
                        Link = $"/auction-details?auctionId={auction.Id}",
                        IsRead = false,
                    };
                }
                else
                {
                    bidding.Member.Wallet += bidding.BiddingPrice;

                    _userDAO.UpdateUser(bidding.Member);
                    notification = new Notification
                    {
                        Description = $"Phiên đấu giá {auction.Title} đã kết thúc",
                        ToUser = bidding.MemberId,
                        Link = $"/auction-details?auctionId={auction.Id}",
                        IsRead = false,
                    };
                }
                notifications.Add(notification);
            }

            notificationForUser = new Notification
            {
                Description = $"Phiên đấu giá {auction.Title} đã kết thúc",
                ToUser = auction.User.Id,
                Link = $"/auction-details?auctionId={auction.Id}",
                IsRead = false,
            };

            _notificationDAO.insertList(notifications);
            _notificationDAO.insert(notificationForUser);
            _logger.LogInformation("Auction end at {time}", DateTime.Now);
        }

        private Timer InitializeTimer(int auctionId, TimeSpan ts)
        {
            Timer timer = null;
            // Create a new timer
            timer = new Timer(_ => DoEndAuction(auctionId, timer), null, ts, TimeSpan.FromMilliseconds(-1));
            return timer;
        }
    }
}
