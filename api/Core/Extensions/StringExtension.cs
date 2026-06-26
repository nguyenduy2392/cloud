using Core.Constants;
using System.Globalization;

namespace Core.Extensions
{
    public static class StringExtension
    {
        public static string ToCurrencyFormat(this decimal value, bool? isDecimal = false)
        {
            if (value.Equals(Numbers.Zero))
            {
                return Numbers.Zero.ToString();
            }

            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            if (isDecimal.HasValue && !isDecimal.Value)
            {
                return value.ToString("#,###", cul.NumberFormat);
            }

            return value.ToString("#,###.##", cul.NumberFormat);
        }

        public static string ToCurrencyFormat(this int value)
        {
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            return value.ToString("#,###", cul.NumberFormat);
        }

    }
}
