using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoMarket.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string BuyerId { get; set; } = string.Empty;

    [ForeignKey("BuyerId")]
    public virtual ApplicationUser? Buyer { get; set; }

    public int? PhotoId { get; set; }

    [ForeignKey("PhotoId")]
    public virtual Photo? Photo { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Type { get; set; } = "Purchase";

    public DateTime PurchaseDate { get; set; } = DateTime.Now;
}
