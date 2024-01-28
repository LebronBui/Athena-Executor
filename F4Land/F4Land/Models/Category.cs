using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? Status { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}
