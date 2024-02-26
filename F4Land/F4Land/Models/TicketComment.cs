using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class TicketComment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Comment { get; set; } = null!;

    public int TicketId { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
