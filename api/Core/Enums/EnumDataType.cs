namespace Core.Enums
{
    public enum EnumDataType
    {
        /// <summary>
        /// Kiểu chữ
        /// </summary>
        Text = 1,

        /// <summary>
        /// Kiểu số
        /// </summary>
        Number = 0,

        /// <summary>
        /// Kiểu ngày tháng
        /// </summary>
        DateTime = 2,

        /// <summary>
        /// Kiểu true / false
        /// </summary>
        Boolean = 3

        /// <summary>
        /// Danh sách chọn 1 giá trị
        /// </summary>
        ,Select = 4

        /// <summary>
        /// Danh sách chọn nhiều giá trị
        /// </summary>
        ,MultiSelect = 5
    }
}
