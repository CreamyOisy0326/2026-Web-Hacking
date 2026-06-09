using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoMarket.Data;
using PhotoMarket.Models;
using PhotoMarket.Services;
using System.Security.Claims;

namespace PhotoMarket.Controllers;

[Authorize]
public class PhotoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;

    public PhotoController(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    // 사진 목록 (Home은 HomeController에서 처리할 수도 있지만, 여기서는 사진 관련 모든 기능 집중)
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var photos = await _context.Photos.Include(p => p.Seller).ToListAsync();
        return View(photos);
    }

    // 사진 상세 정보
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var photo = await _context.Photos
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (photo == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool alreadyPurchased = false;
        if (userId != null)
        {
            alreadyPurchased = await _context.Transactions.AnyAsync(t => t.BuyerId == userId && t.PhotoId == id);
        }

        ViewBag.AlreadyPurchased = alreadyPurchased;
        ViewBag.IsSeller = userId != null && photo.SellerId == userId;

        return View(photo);
    }

    // 사진 업로드 (GET)
    public IActionResult Create()
    {
        return View();
    }

    // 사진 업로드 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Price")] Photo photo, IFormFile imageFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        // 모델에 사용자 ID 할당
        photo.SellerId = userId;

        // 파일 업로드 처리
        if (imageFile != null && imageFile.Length > 0)
        {
            var filePath = await _fileService.UploadFileAsync(imageFile, "photos");
            if (filePath != null)
            {
                photo.FilePath = filePath;
            }
            else
            {
                ModelState.AddModelError("imageFile", "허용되지 않는 파일 형식입니다 (JPG, PNG만 가능).");
            }
        }
        else
        {
            ModelState.AddModelError("imageFile", "이미지 파일을 선택해주세요.");
        }

        // 서버에서 할당한 필드들에 대한 유효성 검사 오류 제거
        ModelState.Remove("SellerId");
        ModelState.Remove("FilePath");
        ModelState.Remove("Seller");

        if (ModelState.IsValid)
        {
            _context.Add(photo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(photo);
    }

    // 내 사진함 (Dashboard)
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userPhotos = await _context.Photos
            .Where(p => p.SellerId == userId)
            .ToListAsync();
        return View(userPhotos);
    }

    // 사진 구매
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(int id)
    {
        var photo = await _context.Photos.Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id == id);
        if (photo == null) return NotFound();

        var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (buyerId == null) return Challenge();

        if (photo.SellerId == buyerId)
        {
            TempData["Error"] = "자신의 사진은 구매할 수 없습니다.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        var buyer = await _context.Users.FindAsync(buyerId);
        var seller = (ApplicationUser?)photo.Seller;

        if (buyer == null || seller == null) return NotFound();

        // 이미 구매했는지 확인
        var alreadyPurchased = await _context.Transactions.AnyAsync(t => t.BuyerId == buyerId && t.PhotoId == id);
        if (alreadyPurchased)
        {
            TempData["Error"] = "이미 구매한 사진입니다.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        int priceInt = (int)photo.Price;

        if (buyer.Credit < priceInt)
        {
            TempData["Error"] = "크레딧이 부족합니다."; // Localization later
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // 트랜잭션 시작 (간단하게)
        buyer.Credit -= priceInt;
        seller.Credit += priceInt;

        var transaction = new Transaction
        {
            BuyerId = buyerId,
            PhotoId = id,
            Amount = photo.Price,
            PurchaseDate = DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["Success"] = "구매가 완료되었습니다.";
        return RedirectToAction(nameof(Details), new { id = id });
    }

    // 내 사진첩 (My Collection)
    public async Task<IActionResult> MyCollection()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var purchasedTransactions = await _context.Transactions
            .Where(t => t.BuyerId == userId)
            .Include(t => t.Photo)
                .ThenInclude(p => p!.Seller)
            .ToListAsync();

        return View(purchasedTransactions ?? new List<Transaction>());
    }

    // 보안 다운로드
    public async Task<IActionResult> Download(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        // 현재 유저가 이 사진을 구매했는지 확인
        var hasPurchased = await _context.Transactions.AnyAsync(t => t.BuyerId == userId && t.PhotoId == id);
        var photo = await _context.Photos.FindAsync(id);

        if (!hasPurchased || photo == null)
        {
            return Unauthorized("사진을 구매하지 않았거나 존재하지 않습니다.");
        }

        // 파일 시스템에서 파일 읽기
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("파일을 찾을 수 없습니다.");
        }

        var fileName = Path.GetFileName(filePath);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/octet-stream", fileName);
    }
}
