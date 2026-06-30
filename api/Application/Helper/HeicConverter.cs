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

        public static bool IsHeic(string extension) => HeicExtensions.Contains(extension);

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
