using Microsoft.EntityFrameworkCore;
using RealEstateAuction.DataModel;
using RealEstateAuction.Enums;
using RealEstateAuction.Models;
using X.PagedList;

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
            return context.Auctions.Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                                    && a.DeleteFlag == false
                                    && DateTime.Now <= a.EndTime)
                                   .Include(a => a.Images)
                                   .Include(a => a.User)
                                   .OrderByDescending(a => a.CreatedTime)
                                   .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                   .Take(pagination.RecordPerPage)
                                   .ToList();
        }

        public IQueryable<Auction> GetAllAuctionApprovedListAuction(Pagination pagination, SearchDataModel? searchData)
        {
            var auctions = context.Auctions.Include(a => a.Images)
                                   .Include(a => a.User)
                                   .Include(a => a.Users)
                                   .Include(a => a.Categories)
                                   .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                                   || a.Status == (int)AuctionStatus.Kết_thúc
                                    && a.DeleteFlag == false
                                    );
            if (searchData.DataCategory.Any() && searchData.DataCategory != null)
            {
                auctions = auctions.Where(a => a.Categories.Any(c => searchData.DataCategory.Contains(c.Id)));
            }

            if (searchData.Keyword != "" && searchData.Keyword != null)
            {
                auctions = auctions.Where(a => a.Title.ToLower().Contains(searchData.Keyword)
                || a.User.FullName.Contains(searchData.Keyword));
            }

            switch (searchData.DataSort)
            {
                case 1:
                    auctions = auctions.OrderBy(x => x.Title);
                    break;
                case 2:
                    auctions = auctions.OrderByDescending(x => x.Title);
                    break;
                case 3:
                    auctions = auctions.OrderBy(x => x.StartTime);
                    break;
                case 4:
                    auctions = auctions.OrderByDescending(x => x.StartTime);
                    break;
                case 5:
                    auctions = auctions.OrderBy(x => x.EndTime);
                    break;
                case 6:
                    auctions = auctions.OrderByDescending(x => x.EndTime);
                    break;
                case 7:
                    auctions = auctions.OrderBy(x => x.StartPrice);
                    break;
                case 8:
                    auctions = auctions.OrderByDescending(x => x.StartPrice);
                    break;
                case 9:
                    auctions = auctions.OrderBy(x => x.EndPrice);
                    break;
                case 10:
                    auctions = auctions.OrderByDescending(x => x.EndPrice);
                    break;
                default:
                    auctions = auctions.OrderByDescending(a => a.CreatedTime);
                    break;
            }

            return auctions;
        }

        public bool AddAuction(Auction auction)
        {
            try
            {
                context.Auctions.Add(auction);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
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
                .Include(a => a.AuctionBiddings)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public Auction? GetAuctionById(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.AuctionBiddings)
                .Include(a => a.Approver)
                .OrderBy(a => a.Id)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public Auction? GetAuctionApproveById(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.AuctionBiddings)
                .Where(a => a.Id == id && a.Approver == null && a.DeleteFlag == false)
                .OrderBy(a => a.Id)
                .FirstOrDefault();
        }

        public Auction? GetAuctionEndById(int id)
        {
            var auction = context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.Approver)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
            if (auction != null)
            {
                auction.AuctionBiddings.Clear();
                Console.WriteLine($"bidding count after clean: {auction.AuctionBiddings.Count()}");
                Console.WriteLine($"user count: {auction.Users.Count()}");
                if (auction.Users.Count > 0)
                {
                    foreach (var user in auction.Users)
                    {
                        var nearestBidding = context.AuctionBiddings
                            .Include(a => a.Member)
                            .Where(ab => ab.MemberId == user.Id && ab.AuctionId == auction.Id)
                            .OrderByDescending(ab => ab.BiddingPrice)
                            .FirstOrDefault();
                    }
                }
                Console.WriteLine($"bidding count after modified: {auction.AuctionBiddings.Count()}");
            }
            return auction ?? null;
        }

        public List<Auction> GetAuctionByUserId(int userId, Pagination pagination)
        {
            return context.Auctions.Where(a => a.UserId == userId && a.DeleteFlag == false)
                                    .OrderByDescending(a => a.Id)
                                    .Include(a => a.Images)
                                    .Include(a => a.Categories)
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
                                   .Include(a => a.AuctionBiddings)
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
                .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                || a.Status == (int)AuctionStatus.Kết_thúc
                && a.DeleteFlag == false
                && DateTime.Now <= a.EndTime)
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .OrderByDescending(a => a.CreatedTime)
                .Take(number)
                .ToList();
        }

        public int CountAuctionApproved()
        {
            //count all auction that have status is approved
            return context.Auctions
                .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                && a.DeleteFlag == false
                && a.StartTime <= DateTime.Now
                && DateTime.Now <= a.EndTime)
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

        public List<User> GetParticipant(int auctionId, Pagination pagination)
        {
            return context.Auctions.Include(a => a.Users)
                                   .FirstOrDefault(a => a.Id == auctionId)
                                   .Users
                                   .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                   .Take(pagination.RecordPerPage)
                                   .ToList();
        }

        public int CountParticipant(int auctionId)
        {
            return context.Auctions.Include(a => a.Users)
                                   .FirstOrDefault(a => a.Id == auctionId)
                                   .Users
                                   .Count();
        }

        public Auction? GetAuctionBiddingById(int id)
        {
            return context.Auctions
                .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                                || a.Status == (int)AuctionStatus.Kết_thúc
                && a.DeleteFlag == false)
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.AuctionBiddings.OrderByDescending(ab => ab.BiddingPrice))
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public List<Auction> GetAuctionsEndingIn1Minute()
        {
            return context.Auctions
                .Where(a => a.EndTime <= DateTime.Now.AddMinutes(1)
                            && a.EndTime >= DateTime.Now
                            && a.Status == (int)AuctionStatus.Chấp_nhân
                            && a.DeleteFlag == false)
                .Include(a => a.Images)
                .Include(a => a.User)
                .OrderBy(a => a.EndTime)
                .ToList();
        }

        public int GetWinnerId(Auction auction)
        {
            var winnerPrice = auction.AuctionBiddings.Max(x => x.BiddingPrice);
            return auction.AuctionBiddings.FirstOrDefault(x => x.BiddingPrice == winnerPrice).MemberId;
        }

        public AuctionBidding GetWinner(Auction auction)
        {
            if (auction.Status >= (int)AuctionStatus.Kết_thúc)
            {
                if (auction.AuctionBiddings.Any())
                {
                    var winnerPrice = auction.AuctionBiddings.Max(x => x.BiddingPrice);
                    return auction.AuctionBiddings.FirstOrDefault(x => x.BiddingPrice == winnerPrice);
                }
            }
            return null;
        }

        public IPagedList<Auction> GetAuctions(int page)
        {
            return context.Auctions
                .Include(x => x.User)
                .Include(x => x.Approver)
                .Include(x => x.Images)
                .ToPagedList(page, 10);
        }

        public Auction? GetAuctionByIdAdmin(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Approver)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
        }

        public Auction? GetApprovedAuction(int id)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.Users)
                .Include(a => a.AuctionBiddings)
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false
                                && a.Status == (int)AuctionStatus.Chờ_phê_duyệt);
        }

        public IPagedList<Auction>? GetAuctionHistoryByUser(int id, int page)
        {
            return context.Auctions
                .Include(a => a.Images)
                .Include(a => a.Users)
                .Include(a => a.AuctionBiddings.OrderByDescending(x => x.BiddingId))
                .Include(a => a.User)
                .Include(a => a.Categories)
                .Where(a => a.Users.Any(x => x.Id == id)
                                && a.DeleteFlag == false)
                .OrderByDescending(a => a.Id)
                .ToPagedList(page, 10);
        }

        public AuctionBidding GetMaxBiddingForAuction(int auctionId)
        {
            AuctionBidding maxBidding = context.AuctionBiddings
                .Where(ab => ab.AuctionId == auctionId)
                .OrderByDescending(ab => ab.BiddingPrice)
                .FirstOrDefault();

            return maxBidding;
        }

    }
}
