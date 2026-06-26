using System.ComponentModel;
using System.Reflection;

namespace Core.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// Lấy giá trị Description (DescriptionAttribute) của một enum value.
        /// </summary>
        public static string GetDescription(this System.Enum value)
        {
            if (value == null) return string.Empty;
            var type = value.GetType();
            var name = value.ToString();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);
            if (field == null) return name;
            var attr = field.GetCustomAttribute<DescriptionAttribute>(inherit: false);
            return attr?.Description ?? name;
        }
    }
}