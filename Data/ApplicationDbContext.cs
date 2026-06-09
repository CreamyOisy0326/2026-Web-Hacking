using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoMarket.Models;

namespace PhotoMarket.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}
