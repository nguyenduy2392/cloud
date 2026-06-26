namespace Core.Common
{
    public class Response
    {
        /// <summary>
        /// Kết quả yêu cầu
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Mã lỗi
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Thông điệp
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Thành công
        /// </summary>
        /// <param name="data">Dữ liệu trả về</param>
        /// <returns></returns>
        public static Response Success(object data = null)
        {
            return new Response
            {
                IsSuccess = true,
                ErrorCode = 0,
                Message = "Success",
                Data = data
            };
        }

        /// <summary>
        /// Thất bại
        /// </summary>
        /// <param name="errorCode">Mã lỗi</param>
        /// <param name="message">Thông điệp</param>
        /// <returns></returns>
        public static Response Fail(string? message = "", int? errorCode = 0)
        {
            if (string.IsNullOrEmpty(message)) message = "Thao tác không thành công.";

            return new Response
            {
                IsSuccess = false,
                ErrorCode = errorCode ?? 0,
                Message = message,
                Data = default
            };
        }
    }
}
