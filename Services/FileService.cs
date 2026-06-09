using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoMarket.Services;

public interface IFileService
{
    Task<string?> UploadFileAsync(IFormFile file, string subDirectory);
}

public class FileService : IFileService
{
    private readonly string _uploadPath;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

    public FileService(IWebHostEnvironment environment)
    {
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string?> UploadFileAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return null;

        var targetFolder = Path.Combine(_uploadPath, subDirectory);
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        // 파일명 난독화 (GUID 사용)
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(targetFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 웹에서 접근 가능한 상대 경로 반환
        return $"/uploads/{subDirectory}/{fileName}";
    }
}
