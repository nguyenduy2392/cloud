namespace Core.Attributes
{
    public class ExcelAttribute : Attribute
    {
        /// <summary>
        /// Tiêu đề cột
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Thứ tự cột
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Độ rộng cột
        /// </summary>
        public int? Width { get; set; }

        public ExcelAttribute(string header, int index, int width)
        {
            Header = header;
            Index = index;
            Width = width;
        }

        public ExcelAttribute(string header, int index)
        {
            Header = header;
            Index = index;
            Width = null;
        }
    }
}
