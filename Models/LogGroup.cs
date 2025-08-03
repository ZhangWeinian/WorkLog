using System.Collections.ObjectModel;

namespace WorkLog.Models
{
	public partial class LogGroup(string title, List<WorkEvent> events) : ObservableCollection<WorkEvent>(events)
	{
		public string Title { get; private set; } = title;
	}
}
