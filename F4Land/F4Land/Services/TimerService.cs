using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using RealEstateAuction.DAL;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Web.Mvc;

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
            var winnerId = _auctionDAO.GetWinnerId(auction);
            var notifications = new List<Notification>();

            //return price for winner
            var winner = _userDAO.GetUserById(winnerId);

            //get the price of the winner
            var price = _auctionDAO.GetMaxPrice(auctionId);

            //keep 10% of the price as a deposit
            var deposit = Math.Round(price * 0.1m);

            //return the rest of the price to the winner
            winner.Wallet += price - deposit;

            //update the winner's wallet
            _userDAO.UpdateUser(winner);

            foreach (var bidding in auction.AuctionBiddings)
            {
                var notification = new Notification();
                if (bidding.MemberId == winnerId)
                {
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
                    var userEnd = _userDAO.GetUserById(bidding.MemberId);

                    userEnd.Wallet += bidding.BiddingPrice;

                    _userDAO.UpdateUser(userEnd);
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
            _notificationDAO.insertList(notifications);
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
