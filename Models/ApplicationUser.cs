using Microsoft.AspNetCore.Identity;

namespace PhotoMarket.Models;

public class ApplicationUser : IdentityUser
{
    public int Credit { get; set; } = 10000;
}
