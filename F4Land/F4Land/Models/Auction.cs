using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Auction
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public decimal StartPrice { get; set; }

    public decimal EndPrice { get; set; }

    public double Area { get; set; }

    public string Address { get; set; } = null!;

    public double Facade { get; set; }

    public string Direction { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int UserId { get; set; }

    public int? ApproverId { get; set; }

    public byte Status { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? Reason { get; set; }

    public virtual User? Approver { get; set; }

    public virtual ICollection<AuctionBidding> AuctionBiddings { get; set; } = new List<AuctionBidding>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
