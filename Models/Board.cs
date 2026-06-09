using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PhotoMarket.Models;

public class Board
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("AuthorId")]
    public virtual ApplicationUser? Author { get; set; }
}
