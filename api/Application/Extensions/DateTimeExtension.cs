namespace Application.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// Lấy ngày đầu tiên và cuối cùng của tuần hiện tại
        /// </summary>
        /// <returns></returns>
        public static (DateTime StartOfWeek, DateTime EndOfWeek) GetCurrentWeek()
        {
            DateTime today = DateTime.Now;

            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff).Date;

            DateTime endOfWeek = startOfWeek.AddDays(6)
                .AddHours(23)
                .AddMinutes(59)
                .AddSeconds(59);

            return (startOfWeek, endOfWeek);
        }

        /// <summary>
        /// Lấy ngày đầu tiên và cuối cùng của tháng
        /// </summary>
        /// <returns></returns>
        public static (DateTime StartOfMonth, DateTime EndOfMonth) GetCurrentMonth()
        {
            DateTime today = DateTime.Now;
            // Ngày đầu tiên của tháng
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            // Ngày cuối cùng của tháng
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return (startOfMonth, endOfMonth);
        }

        public static string ToTextDateFormt(this DateTime date)
        {
            return $"Ngày {date.Day} tháng {date.Month} năm {date.Year}";
        }

        public static string ToDateTimeFormat(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm dd/MM/yyyy");
        }

        /// <summary>
        /// Chỉ lấy ngày tháng
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToDateOnlyFormat(this DateTime dateTime) => dateTime.ToString("dd/MM/yyyy");

        /// <summary>
        /// Chỉ lấy tháng
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToMonthFormat(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return string.Empty;

            return dateTime.Value.ToString("MM/yyyy");
        }

        public static string ToDateFormat(this DateTime date)
        {
            return date.ToString("dd-MM-yyyy");
        }

    }
}
