
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using RealEstateAuction.DAL;
using RealEstateAuction.Models;

namespace RealEstateAuction.Services
{
    public class BackgroundWokerService : BackgroundService
    {
        private readonly TimerService _timerService;
        private ILogger logger;
        private readonly AuctionDAO auctionDAO;

        public BackgroundWokerService(ILogger<TimerService> logger, IUrlHelperFactory urlHelperFactory)
        {
            this.logger = logger;
            _timerService = new TimerService(logger, urlHelperFactory);
            auctionDAO = new AuctionDAO();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // End the auctions that are ending in 1 minutes
                Task endAuctionsTask = Task.Run(() =>
                {
                    try
                    {
                        //Get all auctions that are ending in 1 minute
                        List<Auction> ending = auctionDAO.GetAuctionsEndingIn1Minute();
                        Console.WriteLine($"{DateTime.Now}, Number of auctions: " + ending.Count);
                        //Change status of auction to ended that incomming in 1 minutes
                        _timerService.EndAuction(ending);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                });

                //Wait for 1 min to repeat the process
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
