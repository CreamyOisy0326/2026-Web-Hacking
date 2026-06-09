using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PhotoMarket.Data;
using PhotoMarket.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace PhotoMarket.Controllers;

[Authorize]
public class CreditController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CreditController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _userManager = userManager;
        _localizer = localizer;
    }

    // 충전 페이지 (GET)
    public IActionResult Recharge()
    {
        return View();
    }

    // 충전 처리 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recharge(int amount)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // 크레딧 업데이트
        user.Credit += amount;

        // 트랜잭션 기록
        var transaction = new Transaction
        {
            BuyerId = userId,
            Amount = amount,
            Type = "Charge",
            PurchaseDate = System.DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["RechargeSuccess"].Value;
        return RedirectToAction("Index", "Home");
    }
}
