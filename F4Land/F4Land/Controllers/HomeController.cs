using Microsoft.AspNetCore.Mvc;
using RealEstateAuction.DAL;
using RealEstateAuction.DataModel;
using RealEstateAuction.Models;
using System.Diagnostics;

namespace RealEstateAuction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuctionDAO auctionDAO;
        private readonly Pagination pagination;

        public HomeController(ILogger<HomeController> logger)
        {
            pagination = new Pagination();
            auctionDAO = new AuctionDAO();
            _logger = logger;
        }

        [Route("")]
        [Route("home")]
        public IActionResult Index()
        {
            //get 5 auction recently to display on hompage
            List<Auction> auctionRecent = auctionDAO.GetAuctionRecently(5);

            return View(auctionRecent);
        }

        [Route("list-auction")]
        public IActionResult ListAuction(int? pageNumber)
        {
            if (pageNumber.HasValue)
            {
                pagination.PageNumber = pageNumber.Value;
            }

            //get all auction approved to display on list auction page
            List<Auction> auctions = auctionDAO.GetAllAuctionApproved(pagination);

            int auctionCount = auctionDAO.CountAuctionApproved();
            int pageSize = (int)Math.Ceiling((double)auctionCount / pagination.RecordPerPage);

            ViewBag.currentPage = pagination.PageNumber;
            ViewBag.pageSize = pageSize;

            return View(auctions);
        }

        [Route("auction-details")]
        public IActionResult AuctionDetails(int? auctionId)
        {
            //check auctionId is null or not
            if (auctionId.HasValue)
            {
                Auction auction = auctionDAO.GetAuctionById(auctionId.Value);
                //check auction is found or not
                if (auction != null)
                {
                    //Get max price of auction
                    decimal maxPrice = auctionDAO.GetMaxPrice(auctionId.Value);

                    //Get number of biddings of auction
                    int biddingCount = auctionDAO.GetNumberOfBidding(auctionId.Value);

                    ViewBag.MaxPrice = maxPrice;
                    ViewBag.BiddingCount = biddingCount;

                    return View(auction);
                }
                else
                {
                    TempData["Message"] = "Không tìm thấy phiên đấu giá này";
                    return RedirectToAction("ListAuction");
                }
            }
            else
            {
                return RedirectToAction("ListAuction");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}