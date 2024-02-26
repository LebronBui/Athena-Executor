using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Banking
{
    public int Id { get; set; }

    public string? BankAccount { get; set; }

    public string? AccountName { get; set; }

    public string? BankName { get; set; }

    public bool? Status { get; set; }

    public bool? DeleteFlag { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
