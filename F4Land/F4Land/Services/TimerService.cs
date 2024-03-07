using RealEstateAuction.DAL;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using System.Text;
using System.Threading;

namespace RealEstateAuction.Services
{
    public class TimerService
    {
        private readonly ILogger _logger;
        private readonly AuctionDAO _auctionDAO;

        public TimerService(ILogger logger)
        {
            _logger = logger;
            _auctionDAO = new AuctionDAO();
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
            Auction auction = _auctionDAO.GetAuctionById(auctionId);
            //update status of auction to ended
            auction.Status = (int)AuctionStatus.Kết_thúc;
            //update auction to database
            _auctionDAO.EditAuction(auction);

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
