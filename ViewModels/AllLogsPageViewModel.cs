using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using WorkLog.Models;
using WorkLog.Services;

namespace WorkLog.ViewModels
{
	public partial class AllLogsPageViewModel : ObservableObject
	{
		private List<WorkEvent> _allEventsCache = [];

		[ObservableProperty]
		private ObservableCollection<LogGroup> _groupedEvents = [];

		[ObservableProperty]
		private EventType? _selectedFilterType;

		[ObservableProperty]
		private EventStatus? _selectedFilterStatus;

		[ObservableProperty]
		private DateTime _filterStartDate = DateTime.Now.AddMonths(-1);

		[ObservableProperty]
		private DateTime _filterEndDate = DateTime.Now;

		public AllLogsPageViewModel()
		{
			_ = LoadEventsAsync();
		}

		[RelayCommand]
		private async Task LoadEventsAsync()
		{
			try
			{
				_allEventsCache = await WorkLogDatabase.Instance.GetEventsAsync();
				ApplyFiltersAndGrouping();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("加载失败", $"加载日志时出错: {ex.Message}", "好的");
			}
		}

		[RelayCommand]
		private void ApplyFiltersAndGrouping()
		{
			IEnumerable<WorkEvent> filtered = _allEventsCache;

			filtered = filtered.Where(e => e.Timestamp.Date >= FilterStartDate.Date && e.Timestamp.Date <= FilterEndDate.Date);

			if (SelectedFilterType.HasValue)
			{
				filtered = filtered.Where(e => e.EventType == SelectedFilterType.Value);
			}

			if (SelectedFilterStatus.HasValue)
			{
				filtered = filtered.Where(e => e.Status == SelectedFilterStatus.Value);
			}

			var grouped = filtered
				.OrderByDescending(e => e.Timestamp)
				.GroupBy(e => e.Timestamp.Date)
				.Select(g => new LogGroup(g.Key.ToString("yyyy年MM月dd日"), g.ToList()));

			GroupedEvents.Clear();
			foreach (var group in grouped)
			{
				GroupedEvents.Add(group);
			}
		}

		partial void OnSelectedFilterTypeChanged(EventType? value) => ApplyFiltersAndGrouping();

		partial void OnSelectedFilterStatusChanged(EventStatus? value) => ApplyFiltersAndGrouping();

		partial void OnFilterStartDateChanged(DateTime value) => ApplyFiltersAndGrouping();

		partial void OnFilterEndDateChanged(DateTime value) => ApplyFiltersAndGrouping();
	}
}
