using System.Globalization;
using System.Text;

namespace Application.Helper
{
    public static class StringHelper
    {
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Handle Vietnamese special chars that NormalizationForm doesn't cover
            text = text
                .Replace("đ", "d").Replace("Đ", "D")
                .Replace("ơ", "o").Replace("Ơ", "O")
                .Replace("ư", "u").Replace("Ư", "U");

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static string BuildKeyword(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var noExt = Path.GetFileNameWithoutExtension(name);
            return RemoveDiacritics(noExt);
        }
    }
}
