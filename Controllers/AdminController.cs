using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoMarket.Data;
using System.Threading.Tasks;

namespace PhotoMarket.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 관리자 대시보드
    public async Task<IActionResult> Dashboard()
    {
        var photos = await _context.Photos.Include(p => p.Seller).ToListAsync();
        var boards = await _context.Boards.Include(b => b.Author).ToListAsync();
        var users = await _context.Users.ToListAsync();

        ViewBag.Photos = photos;
        ViewBag.Boards = boards;
        ViewBag.Users = users;

        return View();
    }

    // 사용자 크레딧 수정
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCredit(string userId, int credit)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Credit = credit;
            await _context.SaveChangesAsync();
            TempData["Success"] = "크레딧이 수정되었습니다.";
        }
        return RedirectToAction(nameof(Dashboard));
    }

    // 사진 전역 삭제
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.Photos.FindAsync(id);
        if (photo != null)
        {
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Dashboard));
    }

    // 게시글 전역 삭제
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBoard(int id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board != null)
        {
            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Dashboard));
    }
}
