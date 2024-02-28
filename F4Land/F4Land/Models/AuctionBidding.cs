using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class AuctionBidding
{
    public int BiddingId { get; set; }

    public int AuctionId { get; set; }

    public int MemberId { get; set; }

    public decimal BiddingPrice { get; set; }

    public DateTime TimeBidding { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Member { get; set; } = null!;
}
