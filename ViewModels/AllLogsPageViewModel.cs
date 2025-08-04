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
		private PageState _currentState = PageState.Loading;

		[ObservableProperty]
		private EventType? _selectedFilterType;

		[ObservableProperty]
		private EventStatus? _selectedFilterStatus;

		[ObservableProperty]
		private DateTime _filterStartDate = DateTime.Now.AddMonths(-1);

		[ObservableProperty]
		private DateTime _filterEndDate = DateTime.Now;

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ApplyFiltersAndGroupingCommand))]
		private bool _isLoading = false;

		[ObservableProperty]
		private string _emptyViewMessage = "正在加载日志...";

		public AllLogsPageViewModel()
		{
		}

		[RelayCommand]
		private async Task OnAppearingAsync()
		{
			await LoadEventsAsync();
		}

		[RelayCommand]
		private async Task LoadEventsAsync()
		{
			if (CurrentState == PageState.Loading)
			{
				return;
			}

			CurrentState = PageState.Loading;

			await Task.Delay(100);
			try
			{
				var dataLoadingTask = WorkLogDatabase.Instance.GetEventsAsync();
				var minimumDelayTask = Task.Delay(500);

				await Task.WhenAll(minimumDelayTask, dataLoadingTask);

				_allEventsCache = dataLoadingTask.Result;
				ApplyFiltersAndGrouping();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("加载失败", $"加载日志时出错: {ex.Message}", "好的");
				CurrentState = PageState.Empty;
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
				.Select(g => new LogGroup(g.Key.ToString("yyyy年MM月dd日"), [.. g]));

			GroupedEvents.Clear();
			foreach (var group in grouped)
			{
				GroupedEvents.Add(group);
			}

			if (GroupedEvents.Count > 0)
			{
				CurrentState = PageState.Normal;
			}
			else
			{
				CurrentState = PageState.Empty;

				if (SelectedFilterType.HasValue || SelectedFilterStatus.HasValue)
				{
					EmptyViewMessage = "在当前筛选条件下没有找到日志。";
				}
				else
				{
					EmptyViewMessage = "你还没有记录任何日志。快去工作台添加第一条吧！";
				}
			}
		}

		partial void OnSelectedFilterTypeChanged(EventType? value) => ApplyFiltersAndGrouping();

		partial void OnSelectedFilterStatusChanged(EventStatus? value) => ApplyFiltersAndGrouping();

		partial void OnFilterStartDateChanged(DateTime value) => ApplyFiltersAndGrouping();

		partial void OnFilterEndDateChanged(DateTime value) => ApplyFiltersAndGrouping();
	}
}
