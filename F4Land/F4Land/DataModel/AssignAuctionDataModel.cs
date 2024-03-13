using System.ComponentModel.DataAnnotations;

namespace RealEstateAuction.DataModel
{
    public class AssignAuctionDataModel
    {
        [Required]
        public int StaffId { get; set; }
        [Required]
        public int AuctionId { get; set; }
    }
}
