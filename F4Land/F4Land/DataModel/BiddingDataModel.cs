using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class BiddingDataModel
    {
        [Required]
        public int AuctionId { get; set; }
        [Required]
        public int MemberId { get; set; }
        [Required]
        public decimal BiddingPrice { get; set; }
    }
}
