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
		private DateTime _filterStartDate = DateTime.Today.AddMonths(-1);

		[ObservableProperty]
		private DateTime _filterEndDate = DateTime.Today;

		[ObservableProperty]
		private string _emptyViewTitle = "正在加载...";

		[ObservableProperty]
		private string _emptyViewSubtitle = "请稍候...";

		private CancellationTokenSource? _filterDebounceCts;

		public AllLogsPageViewModel()
		{
		}

		[RelayCommand]
		private async Task OnAppearingAsync()
		{
			if (_allEventsCache.Count == 0)
			{
				await LoadEventsAsync();
			}
		}

		private async Task LoadEventsAsync()
		{
			CurrentState = PageState.Loading;

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
				EmptyViewTitle = "加载失败";
				EmptyViewSubtitle = "无法从数据库读取日志。";
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
				if (_allEventsCache.Count == 0)
				{
					EmptyViewTitle = "你还没有任何日志";
					EmptyViewSubtitle = "快去工作台添加第一条吧！";
				}
				else
				{
					EmptyViewTitle = "没有找到符合条件的日志";
					EmptyViewSubtitle = "请尝试放宽筛选条件或日期范围。";
				}
			}
		}

		private void OnFilterChanged()
		{
			_filterDebounceCts?.Cancel();
			_filterDebounceCts?.Dispose();
			_filterDebounceCts = new CancellationTokenSource();

			Task.Delay(300, _filterDebounceCts.Token)
				.ContinueWith(t =>
				{
					if (t.IsCanceled)
					{
						return;
					}
					MainThread.BeginInvokeOnMainThread(ApplyFiltersAndGrouping);
				}, TaskScheduler.Default);
		}

		partial void OnSelectedFilterTypeChanged(EventType? value) => OnFilterChanged();

		partial void OnSelectedFilterStatusChanged(EventStatus? value) => OnFilterChanged();

		partial void OnFilterStartDateChanged(DateTime value) => OnFilterChanged();

		partial void OnFilterEndDateChanged(DateTime value) => OnFilterChanged();
	}
}
