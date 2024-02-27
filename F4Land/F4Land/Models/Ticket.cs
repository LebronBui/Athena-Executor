using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? StaffId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public byte Status { get; set; }

    public virtual User? Staff { get; set; }

    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();

    public virtual ICollection<TicketImage> TicketImages { get; set; } = new List<TicketImage>();

    public virtual User User { get; set; } = null!;
}
