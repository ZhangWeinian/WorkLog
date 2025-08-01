using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace WorkLog.Converters
{
	public class EnumToDescriptionConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return string.Empty;
			}

			if (value is not Enum anEnum)
			{
				return value.ToString();
			}

			FieldInfo? fieldInfo = anEnum.GetType().GetField(anEnum.ToString());
			if (fieldInfo != null)
			{
				var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
				return attributes.Length > 0 ? attributes[0].Description : anEnum.ToString();
			}

			return anEnum.ToString();
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
