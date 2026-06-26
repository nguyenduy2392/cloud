namespace Application.Options
{
    public class ConnectionStrings
    {
        /// <summary>
        /// Chuỗi kết nối mặc định
        /// </summary>
        public string DefaultConnection { get; set; }

        /// <summary>
        /// Mongo connection
        /// </summary>
        public string MongoConnection { get; set; }

        /// <summary>
        /// Mongo database
        /// </summary>
        public string MongoDatabase { get; set; }
    }
}
