using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoMarket.Data;
using PhotoMarket.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PhotoMarket.Controllers;

[Authorize]
public class BoardController : Controller
{
    private readonly ApplicationDbContext _context;

    public BoardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 게시글 목록 (전체 공개)
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var boards = await _context.Boards
            .Include(b => b.Author)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(boards);
    }

    // 게시글 상세 (전체 공개)
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var board = await _context.Boards
            .Include(b => b.Author)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (board == null) return NotFound();

        return View(board);
    }

    // 게시글 작성 (GET)
    public IActionResult Create()
    {
        return View();
    }

    // 게시글 작성 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Content")] Board board)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            TempData["Error"] = "사용자 인증에 실패했습니다. 다시 로그인해주세요.";
            return Challenge();
        }

        // 모델에 사용자 ID 및 작성일 명시적 할당
        board.AuthorId = userId;
        board.CreatedAt = DateTime.Now;

        // 서버에서 할당한 필드들에 대한 유효성 검사 오류 제거
        ModelState.Remove("AuthorId");
        ModelState.Remove("Author");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(board);
                await _context.SaveChangesAsync();
                TempData["Success"] = "게시글이 성공적으로 등록되었습니다!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Save Error: {ex.Message}");
                ModelState.AddModelError("", "데이터베이스 저장 중 오류가 발생했습니다.");
            }
        }

        // 유효성 검사 실패 시 에러 로그 출력
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        Console.WriteLine("Board Create Validation Errors: " + string.Join(", ", errors));
        TempData["Error"] = "입력값을 확인해주세요.";

        return View(board);
    }

    // 게시글 수정 (GET)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var board = await _context.Boards.FindAsync(id);
        if (board == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (board.AuthorId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(board);
    }

    // 게시글 수정 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content")] Board board)
    {
        if (id != board.Id) return NotFound();

        var existingBoard = await _context.Boards.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (existingBoard == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (existingBoard.AuthorId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                board.AuthorId = existingBoard.AuthorId;
                board.CreatedAt = existingBoard.CreatedAt;
                _context.Update(board);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoardExists(board.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(board);
    }

    // 게시글 삭제 (POST)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var board = await _context.Boards.FindAsync(id);
        if (board == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (board.AuthorId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BoardExists(int id)
    {
        return _context.Boards.Any(e => e.Id == id);
    }
}
