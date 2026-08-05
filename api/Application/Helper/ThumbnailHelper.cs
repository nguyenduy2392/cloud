using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Application.Helper
{
    /// <summary>
    /// Tạo ảnh thumbnail (400x400, JPEG) cho file ảnh (mọi định dạng ImageSharp decode được) và video
    /// (qua ffmpeg, đã cài trong Dockerfile — cùng cách HeicConverter dùng heif-convert).
    /// Luôn best-effort: lỗi thì log cảnh báo, không throw — không được làm hỏng luồng upload chính.
    /// </summary>
    public static class ThumbnailHelper
    {
        private const int MaxSize = 400;

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff", ".tif"
        };

        private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mov", ".avi", ".mkv", ".webm", ".m4v", ".3gp", ".wmv", ".flv", ".mpeg", ".mpg"
        };

        public static bool IsImage(string extension) => ImageExtensions.Contains(extension);
        public static bool IsVideo(string extension) => VideoExtensions.Contains(extension);
        public static bool IsSupported(string extension) => IsImage(extension) || IsVideo(extension);

        /// <summary>Tạo thumbnail tại thumbPath (luôn JPEG) từ file nguồn. Trả false nếu không phải ảnh/video
        /// hoặc quá trình tạo thất bại — không throw.</summary>
        public static async Task<bool> GenerateAsync(string sourcePath, string extension, string thumbPath, ILogger logger)
        {
            try
            {
                if (IsImage(extension))
                    return await GenerateFromImageAsync(sourcePath, thumbPath);

                if (IsVideo(extension))
                    return await GenerateFromVideoAsync(sourcePath, thumbPath, logger);

                return false;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Không thể tạo thumbnail cho [{Path}].", sourcePath);
                return false;
            }
        }

        private static async Task<bool> GenerateFromImageAsync(string sourcePath, string thumbPath)
        {
            using var image = await Image.LoadAsync(sourcePath);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxSize, MaxSize)
            }));
            await image.SaveAsync(thumbPath, new JpegEncoder { Quality = 80 });
            return true;
        }

        private static async Task<bool> GenerateFromVideoAsync(string sourcePath, string thumbPath, ILogger logger)
        {
            // -ss TRƯỚC -i để ffmpeg seek nhanh (input seeking) thay vì decode từ đầu file — quan trọng
            // với video dung lượng lớn (upload cho phép tới 10GB).
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                ArgumentList =
                {
                    "-y",
                    "-ss", "00:00:01.000",
                    "-i", sourcePath,
                    "-vframes", "1",
                    "-vf", $"scale='min({MaxSize},iw)':'min({MaxSize},ih)':force_original_aspect_ratio=decrease",
                    thumbPath
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 || !File.Exists(thumbPath))
            {
                logger.LogWarning("ffmpeg tạo thumbnail thất bại (exit {Code}): {Error}", process.ExitCode, stderr);
                return false;
            }

            return true;
        }
    }
}
