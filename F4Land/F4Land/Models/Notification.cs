using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int? ToUser { get; set; }

    public string? Link { get; set; }

    public bool? IsRead { get; set; }

    public virtual User? ToUserNavigation { get; set; }
}
