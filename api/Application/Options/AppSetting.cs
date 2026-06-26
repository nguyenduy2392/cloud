namespace Application.Options
{
    /// <summary>
    /// Cài đặt thông số token
    /// </summary>
    public class AppSetting
    {
        public string Token { get; set; }

        public int ExpiredIn { get; set; }

        public string Secret { get; set; }
    }

    public class ServerSetting
    {
        public string Address { get; set; }


        public string Id { get; set; }


        public string Password { get; set; }
    }
}
