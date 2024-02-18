using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Image
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}
