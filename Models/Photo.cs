using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PhotoMarket.Models;

public class Photo
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "TitleRequired")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required(ErrorMessage = "PriceRequired")]
    [Range(1, (double)decimal.MaxValue, ErrorMessage = "PriceRange")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Required]
    public string SellerId { get; set; } = string.Empty;

    [ForeignKey("SellerId")]
    public virtual ApplicationUser? Seller { get; set; }
}
