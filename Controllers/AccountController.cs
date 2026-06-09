using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PhotoMarket.Controllers;

public class AccountController : Controller
{
    // 비밀번호 찾기 (GET)
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // 비밀번호 찾기 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "이메일을 입력해주세요.");
            return View();
        }

        // 실제 이메일 발송 대신 재설정 페이지로 바로 이동하는 워크플로우
        return RedirectToAction(nameof(ResetPassword), new { email = email });
    }

    // 비밀번호 재설정 (GET)
    public IActionResult ResetPassword(string email)
    {
        ViewBag.Email = email;
        return View();
    }

    // 비밀번호 재설정 (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ResetPassword(string email, string newPassword)
    {
        // 실제 비밀번호 변경 로직은 Identity 연동이 필요하나, 
        // 우선 워크플로우만 구현하라는 요청에 따라 성공 메시지 후 로그인으로 이동
        ViewBag.Message = "비밀번호가 성공적으로 재설정되었습니다. 다시 로그인해주세요.";
        return View("ResetPasswordSuccess");
    }
}
