using Core.Constants;
using System.Globalization;

namespace Application.Extensions
{
    public static class StringExtension
    {
        public static string ToCurrency(this decimal value, bool? isDecimal = false)
        {
            if (value.Equals(Numbers.Zero))
            {
                return Numbers.Zero.ToString();
            }

            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");
            if (!isDecimal.Value)
            {
                return value.ToString("#,###", cul.NumberFormat);
            }

            return value.ToString("#,###.##", cul.NumberFormat);
        }

        public static decimal ToDecimal(this string input)
        {
            decimal result = 0;
            decimal.TryParse(input, out result);

            return result;
        }

        /// <summary>
        /// Xóa các kí tự chữ, giữ lại ký tự số
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToRemoveLetters(this string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }
    }
}
