using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class AuctionParticipant
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AuctionId { get; set; }

    public decimal BiddingPrice { get; set; }

    public DateTime BiddingTime { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
