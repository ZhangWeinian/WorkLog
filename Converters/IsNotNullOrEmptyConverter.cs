using System.Globalization;

namespace WorkLog.Converters
{
	public class IsNotNullOrEmptyConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is string str)
			{
				return !string.IsNullOrWhiteSpace(str);
			}

			return value != null;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
