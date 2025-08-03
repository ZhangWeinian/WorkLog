namespace WorkLog.Models
{
	public static class EventTypeExtensions
	{
		public static List<EventType?> All
		{
			get;
		} = [.. Enum.GetValues<EventType>().Cast<EventType?>().ToList().Prepend(null)];
	}

	public static class EventStatusExtensions
	{
		public static List<EventStatus?> All
		{
			get;
		} = [.. Enum.GetValues<EventStatus>().Cast<EventStatus?>().ToList().Prepend(null)];
	}
}
