using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime Dob { get; set; }

    public string Address { get; set; } = null!;

    public string? Avatar { get; set; }

    public int RoleId { get; set; }

    public string? Description { get; set; }

    public byte? Status { get; set; }

    public decimal Wallet { get; set; }

    public virtual ICollection<Auction> AuctionApprovers { get; set; } = new List<Auction>();

    public virtual ICollection<AuctionBidding> AuctionBiddings { get; set; } = new List<AuctionBidding>();

    public virtual ICollection<Auction> AuctionUsers { get; set; } = new List<Auction>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    public virtual ICollection<Ticket> TicketStaffs { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketUsers { get; set; } = new List<Ticket>();

    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}
