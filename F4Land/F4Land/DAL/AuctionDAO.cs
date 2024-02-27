using Microsoft.EntityFrameworkCore;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;

namespace RealEstateAuction.DAL
{
    public class AuctionDAO
    {
        private readonly RealEstateContext context;
        public AuctionDAO()
        {
            context = new RealEstateContext();
        }

        //get all auction that have status is approved
        public List<Auction> GetAllAuctionApproved(Pagination pagination)
        {
            return context.Auctions.Where(a => a.Status == (int)AuctionStatus.Chấp_nhân && a.DeleteFlag == false)
                                   .Include(a => a.Images)
                                   .OrderByDescending(a => a.CreatedTime)
                                   .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                   .Take(pagination.RecordPerPage)
                                   .ToList();
        }

        public bool AddAuction(Auction auction)
        {
            try
            {
                context.Auctions.Add(auction);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool EditAuction(Auction auction)
        {
            try
            {
                context.Auctions.Update(auction);
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Auction? GetAuctionStaffById(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.Approver)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public Auction? GetAuctionById(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public List<Auction> GetAuctionByUserId(int userId, Pagination pagination)
        {
            return context.Auctions.Where(a => a.UserId == userId && a.DeleteFlag == false)
                                    .Include(a => a.Images)
                                    .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                    .Take(pagination.RecordPerPage)
                                    .ToList();
        }

        public int CountAuctionByUserId(int userId)
        {
            return context.Auctions.Where(a => a.UserId == userId && a.DeleteFlag == false).Count();
        }

        public bool DeleteAuction(Auction auction)
        {
            try
            {
                auction.DeleteFlag = true;
                EditAuction(auction);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Auction> GetAuctionByStaffId(int staffId, Pagination pagination)
        {
            return context.Auctions.Where(a => a.ApproverId == staffId && a.DeleteFlag == false)
                                   .Include(a => a.Images)
                                   .OrderBy(a => a.Status)
                                   .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                   .Take(pagination.RecordPerPage)
                                   .ToList();
        }

        public int CountAuctionByStaffId(int staffId)
        {
            return context.Auctions.Where(a => a.ApproverId == staffId && a.DeleteFlag == false).Count();
        }

        public List<Auction> GetAuctionRecently(int number)
        {
            return context.Auctions
                .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân && a.DeleteFlag == false)
                .Include(a => a.Images)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedTime)
                .Take(number)
                .ToList();
        }

        public int CountAuctionApproved()
        {
            //count all auction that have status is approved
            return context.Auctions
                .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân && a.DeleteFlag == false)
                .Count();
        }

        public bool IsUserJoinedAuction(User user, int id)
        {
            return context.Auctions.Where(a => a.Id == id)
                .Any(a => a.Users.Contains(user));
        }

        public decimal GetMaxPrice(int id)
        {
            //Check if auction have no bidding
            bool isHaveBidding = context.AuctionBiddings.Where(ab => ab.AuctionId == id).Count() == 0;
            if (isHaveBidding)
            {
                return context.Auctions.FirstOrDefault(a => a.Id == id).StartPrice;
            }
            return context.AuctionBiddings
                .Where(ab => ab.AuctionId == id)
                .Max(ab => ab.BiddingPrice);
        }

        public int GetNumberOfBidding(int id)
        {
            return context.AuctionBiddings
                .Where(ab => ab.AuctionId == id)
                .Count();
        }
    }
}
