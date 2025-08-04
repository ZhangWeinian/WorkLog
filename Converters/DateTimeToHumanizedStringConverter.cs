using System.Globalization;

namespace WorkLog.Converters
{
	public class DateTimeToHumanizedStringConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is not DateTime timestamp)
			{
				return value;
			}

			var today = DateTime.Today;
			var date = timestamp.Date;

			if (date == today)
			{
				return "今天";
			}
			if (date == today.AddDays(-1))
			{
				return "昨天";
			}
			if (date == today.AddDays(-2))
			{
				return "前天";
			}

			var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
			if (today.DayOfWeek == DayOfWeek.Sunday)
			{
				startOfWeek = startOfWeek.AddDays(-7);
			}
			if (date >= startOfWeek)
			{
				return culture.DateTimeFormat.GetDayName(date.DayOfWeek);
			}

			if (date.Year == today.Year)
			{
				return date.ToString("M", culture);
			}

			return date.ToString("yyyy年M月d日", culture);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
