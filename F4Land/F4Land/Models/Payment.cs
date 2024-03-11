using System;
using System.Collections.Generic;

namespace RealEstateAuction.Models;

public partial class Payment
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public int? UserId { get; set; }

    public byte? Status { get; set; }

    public string? Code { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? UserBankName { get; set; }

    public string? UserBankAccount { get; set; }

    public byte? Type { get; set; }

    public int? BankId { get; set; }

    public virtual Banking? Bank { get; set; }

    public virtual User? User { get; set; }
}
