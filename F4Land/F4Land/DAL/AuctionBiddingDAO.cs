using Microsoft.EntityFrameworkCore;
using RealEstateAuction.DataModel;
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

        public List<AuctionBidding> GetParticipantByAuctionId(int auctionId, Pagination pagination)
        {
            using (var context = new RealEstateContext())
            {
                return context.AuctionBiddings
                    .Where(ab => ab.AuctionId == auctionId)
                    .OrderByDescending(ab => ab.BiddingPrice)
                    .Skip(pagination.RecordPerPage * (pagination.PageNumber - 1))
                    .Take(pagination.RecordPerPage)
                    .Include(ab => ab.Member)
                    .ToList();
            }
        }

        public int CountParticipant(int auctionId)
        {
            using (var context = new RealEstateContext())
            {
                return context.AuctionBiddings
                    .Where(ab => ab.AuctionId == auctionId)
                    .Count();
            }
        }

        public List<User> GetJoiningByAuctionId(int auctionId, Pagination pagination)
        {
            using (var context = new RealEstateContext())
            {
                //get list users in auction
                return context.Auctions
                    .Where(a => a.Id == auctionId)
                    .SelectMany(a => a.Users)
                    .Skip(pagination.RecordPerPage * (pagination.PageNumber - 1))
                    .Take(pagination.RecordPerPage)
                    .ToList();

            }
        }
        public int CountJoining(int auctionId)
        {
            using (var context = new RealEstateContext())
            {
                return context.Auctions
                    .Where(a => a.Id == auctionId)
                    .SelectMany(a => a.Users)
                    .Count();
            }
        }

        public AuctionBidding? GetLastBiddingByUser(int auctionId, int memberId)
        {
            using (var context = new RealEstateContext())
            {
                return context.AuctionBiddings
                    .Where(ab => ab.AuctionId == auctionId && ab.MemberId == memberId)
                    .OrderByDescending(ab => ab.BiddingPrice)
                    .FirstOrDefault();
            }
        }
    }
}
