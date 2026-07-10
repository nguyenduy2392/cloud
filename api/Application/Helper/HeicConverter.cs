using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Application.Helper
{
    /// <summary>
    /// Convert HEIC/HEIF sang JPEG bằng CLI heif-convert (libheif-examples).
    /// .NET image libs (Magick.NET, SkiaSharp...) không bundle sẵn HEIC decoder do vướng bản quyền HEVC,
    /// nên dùng heif-convert đã cài qua apt trong Dockerfile.
    /// </summary>
    public static class HeicConverter
    {
        private static readonly HashSet<string> HeicExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".heic", ".heif"
        };

        private static readonly Dictionary<string, string> ImageContentTypeExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/jpg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/gif"] = ".gif",
            ["image/webp"] = ".webp",
            ["image/bmp"] = ".bmp",
        };

        public static bool IsHeic(string extension) => HeicExtensions.Contains(extension);

        /// <summary>
        /// Trả về đuôi file tương ứng với ContentType nếu là 1 định dạng ảnh phổ biến (jpeg/png/...).
        /// Một số app mobile (image picker iOS) tự convert HEIC -> JPEG nhưng vẫn giữ nguyên tên file gốc .HEIC,
        /// nên không thể tin tưởng hoàn toàn vào đuôi file để biết định dạng thực tế.
        /// </summary>
        public static string? ExtensionFromContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return null;
            var normalized = contentType.Split(';')[0].Trim();
            return ImageContentTypeExtensions.TryGetValue(normalized, out var ext) ? ext : null;
        }

        public static async Task<bool> ConvertToJpegAsync(string sourcePath, string destPath, ILogger logger)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "heif-convert",
                    ArgumentList = { sourcePath, destPath },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null) return false;

                var stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0 || !File.Exists(destPath))
                {
                    logger.LogWarning("heif-convert thất bại (exit {Code}): {Error}", process.ExitCode, stderr);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Không thể chạy heif-convert để convert HEIC sang JPEG.");
                return false;
            }
        }
    }
}
