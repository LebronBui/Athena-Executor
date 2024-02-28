using Microsoft.EntityFrameworkCore;
using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class AuctionBiddingDAO
    {
        public List<AuctionBidding> GetAuctionBiddings(int auctionId)
        {
            using (var context = new RealEstateContext())
            {
                return context.AuctionBiddings
                    .Where(ab => ab.AuctionId == auctionId)
                    .ToList();
            }
        }

        public bool AddAuctionBidding(AuctionBidding auctionBidding)
        {
            using (var context = new RealEstateContext())
            {
                try
                {
                    context.AuctionBiddings.Add(auctionBidding);
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
}
