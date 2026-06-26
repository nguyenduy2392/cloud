using Microsoft.AspNetCore.Http;

namespace Application.Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// Tải lên tệp
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <param name="tenant">Định danh đơn vị (thư mục lưu trữ riêng theo đơn vị)</param>
        /// <returns></returns>
        public static async Task<string> ToUploadAsync(this IFormFile file, string folder, string tenant)
        {
            var ext = Path.GetExtension(file.FileName);
            var baseName = Path.GetFileNameWithoutExtension(file.FileName);

            // Sanitize: bỏ ký tự không hợp lệ trong tên file
            var invalidChars = Path.GetInvalidFileNameChars();
            baseName = new string(baseName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "file";

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", tenant, folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = baseName;
            var filePath = Path.Combine(folderPath, $"{fileName}{ext}");

            // Nếu trùng tên → thêm _timestamp
            if (File.Exists(filePath))
            {
                fileName = $"{baseName}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                filePath = Path.Combine(folderPath, $"{fileName}{ext}");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
                return $"{tenant}/{folder}/{fileName}{ext}";
            }
        }
    }
}
