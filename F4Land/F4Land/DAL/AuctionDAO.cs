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
                                    && a.StartTime <= DateTime.Now
                                    && DateTime.Now <= a.EndTime)
                                   .Include(a => a.Images)
                                   .Include(a => a.User)
                                   .OrderByDescending(a => a.CreatedTime)
                                   .Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
                                   .Take(pagination.RecordPerPage)
                                   .ToList();
        }

        public List<Auction> GetAllAuctionApprovedListAuction(Pagination pagination, SearchDataModel? searchData)
        {
            var auctions = context.Auctions.Include(a => a.Images)
                                   .Include(a => a.User)
                                   .Include(a => a.Categories)
                                   .Where(a => a.Status == (int)AuctionStatus.Chấp_nhân
                                    && a.DeleteFlag == false
                                    && a.StartTime <= DateTime.Now
                                    && DateTime.Now <= a.EndTime
                                    );
            if (searchData.DataCategory.Any() || searchData?.DataCategory != null || searchData?.DataSort != null)
            {
                if (searchData.DataCategory.Any())
                {
                    auctions = auctions.Where(a => a.Categories.Any(c => searchData.DataCategory.Contains(c.Id)));
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
            }
            else
            {
                auctions = auctions.OrderByDescending(a => a.CreatedTime);
            }

            return auctions.Skip((pagination.PageNumber - 1) * pagination.RecordPerPage)
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
                .FirstOrDefault(a => a.Id == id
                                && a.DeleteFlag == false);
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
                && a.DeleteFlag == false
                && a.StartTime <= DateTime.Now
                && DateTime.Now <= a.EndTime)
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
                .Where(a => a.StartTime <= DateTime.Now
                && DateTime.Now <= a.EndTime
                && a.Status == (int)AuctionStatus.Chấp_nhân
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
    }
}
