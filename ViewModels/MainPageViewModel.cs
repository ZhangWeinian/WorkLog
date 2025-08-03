using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using WorkLog.Models;
using WorkLog.Services;

namespace WorkLog.ViewModels
{
	public partial class MainPageViewModel : ObservableObject
	{
		public List<EventType> EventTypes
		{
			get;
		}

		public List<EventStatus> Statuses
		{
			get;
		}

		[ObservableProperty]
		private ObservableCollection<WorkEvent> _events = [];

		[ObservableProperty]
		private EventType _selectedEventType;

		[ObservableProperty]
		private EventStatus _selectedStatus;

		[ObservableProperty]
		private string _titleText = string.Empty;

		[ObservableProperty]
		private string _descriptionText = string.Empty;

		[ObservableProperty]
		private string _remarksText = string.Empty;

		[ObservableProperty]
		private DateTime _selectedDate = DateTime.Today;

		[ObservableProperty]
		private WorkEvent? _selectedEvent;

		[ObservableProperty]
		private string _copyButtonText = "复制";

		[ObservableProperty]
		private string _saveButtonText = "保存";

		private List<WorkEvent> _allEvents = [];

		[ObservableProperty]
		private string _searchText = string.Empty;

		private CancellationTokenSource? _debounceCts;

		public MainPageViewModel()
		{
			EventTypes = [.. Enum.GetValues<EventType>()];
			Statuses = [.. Enum.GetValues<EventStatus>()];
			ClearForm();
		}

		[RelayCommand]
		private async Task LoadEventsAsync()
		{
			try
			{
				_allEvents = await WorkLogDatabase.Instance.GetEventsAsync();
				FilterEvents();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("加载失败", $"加载日志时出错: {ex.Message}", "好的");
			}
		}

		private void FilterEvents()
		{
			var searchText = SearchText.Trim();

			if (string.IsNullOrWhiteSpace(searchText))
			{
				var sortedAll = _allEvents.OrderByDescending(e => e.Timestamp);
				Events.Clear();
				foreach (var ev in sortedAll)
				{
					Events.Add(ev);
				}
				return;
			}

			IEnumerable<WorkEvent> filtered = [];
			bool searchPerformed = false;

			if (searchText.All(char.IsDigit))
			{
				switch (searchText.Length)
				{
					// 格式: yyyyMMdd (例如 20250903)
					case 8:
					{
						if (int.TryParse(searchText.AsSpan(0, 4), out int year8) &&
							int.TryParse(searchText.AsSpan(4, 2), out int month8) &&
							int.TryParse(searchText.AsSpan(6, 2), out int day8) &&
							month8 >= 1 && month8 <= 12 && day8 >= 1 && day8 <= 31)
						{
							filtered = _allEvents.Where(e => e.Timestamp.Year == year8 &&
															 e.Timestamp.Month == month8 &&
															 e.Timestamp.Day == day8);
							searchPerformed = true;
						}
						break;
					}

					// 格式: yyyyMM (例如 202507)
					case 6:
					{
						if (int.TryParse(searchText.AsSpan(0, 4), out int year6) &&
						int.TryParse(searchText.AsSpan(4, 2), out int month6) &&
						month6 >= 1 && month6 <= 12)
						{
							filtered = _allEvents.Where(e => e.Timestamp.Year == year6 &&
															 e.Timestamp.Month == month6);
							searchPerformed = true;
						}
						break;
					}

					// 格式: yyyy (例如 2025)
					case 4:
					{
						if (int.TryParse(searchText, out int year4))
						{
							filtered = _allEvents.Where(e => e.Timestamp.Year == year4);
							searchPerformed = true;
						}
						break;
					}
				}
			}

			if (!searchPerformed)
			{
				filtered = _allEvents.Where(e =>
					e.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
					e.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
					(e.Remarks != null && e.Remarks.Contains(searchText, StringComparison.OrdinalIgnoreCase))
				  );
			}

			var sorted = filtered.OrderByDescending(e => e.Timestamp);
			Events.Clear();
			foreach (var ev in sorted)
			{
				Events.Add(ev);
			}
		}

		[RelayCommand]
		private async Task SaveAsync()
		{
			if (string.IsNullOrWhiteSpace(TitleText))
			{
				await Shell.Current.DisplayAlert("错误", "日志标题不能为空。", "好的");
				return;
			}

			try
			{
				var eventToSave = SelectedEvent ?? new WorkEvent();
				eventToSave.Title = TitleText;
				eventToSave.Description = DescriptionText;
				eventToSave.Remarks = RemarksText;
				eventToSave.EventType = SelectedEventType;
				eventToSave.Status = SelectedStatus;

				if (eventToSave.Id == 0)
				{
					eventToSave.Timestamp = SelectedDate.Date.Add(DateTime.Now.TimeOfDay);
				}

				eventToSave.ProjectId = 1;
				eventToSave.TaskId = 1;

				await WorkLogDatabase.Instance.SaveEventAsync(eventToSave);

				ClearForm();
				await LoadEventsAsync();
				await AnimateSaveButtonAsync();
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("保存失败", $"发生了一个错误: {ex.Message}", "好的");
			}
		}

		private async Task AnimateSaveButtonAsync()
		{
			string originalText = "保存";
			string savingText = "正在保存...";
			string successText = "✓ 保存成功";

			SaveButtonText = savingText;
			await Task.Delay(400);

			SaveButtonText = successText;
			await Task.Delay(1200);

			SaveButtonText = originalText;
		}

		[RelayCommand]
		private void ClearForm()
		{
			SelectedEvent = null;
			TitleText = string.Empty;
			DescriptionText = string.Empty;
			RemarksText = string.Empty;
			SelectedEventType = EventType.BugFix;
			SelectedStatus = EventStatus.ToDo;
			SelectedDate = DateTime.Today;

			SaveButtonText = "保存";
			CopyButtonText = "复制";
		}

		[RelayCommand(CanExecute = nameof(CanDelete))]
		private async Task DeleteAsync()
		{
			if (SelectedEvent == null)
			{
				return;
			}

			bool answer = await Shell.Current.DisplayAlert("确认删除", $"你确定要删除这条日志吗？\n\n{SelectedEvent.Title}\n\n'{SelectedEvent.Description}'", "是，删除", "否");
			if (answer)
			{
				try
				{
					await WorkLogDatabase.Instance.DeleteEventAsync(SelectedEvent);
					SelectedEvent = null;
					await LoadEventsAsync();
				}
				catch (Exception ex)
				{
					await Shell.Current.DisplayAlert("删除失败", $"发生了一个错误: {ex.Message}", "好的");
				}
			}
		}

		private bool CanDelete()
		{
			return SelectedEvent != null && SelectedEvent.Id != 0;
		}

		[RelayCommand(CanExecute = nameof(CanCopy))]
		private async Task CopyAsync()
		{
			if (string.IsNullOrWhiteSpace(DescriptionText))
			{
				return;
			}

			var contentToCopy = $"日期：{SelectedDate.ToShortDateString()}\n标题：{TitleText}\n问题：{DescriptionText}\n备注：{RemarksText ?? "无"}";
			await Clipboard.Default.SetTextAsync(contentToCopy);
			await AnimateCopyButtonAsync();
		}

		private bool CanCopy()
		{
			return !string.IsNullOrWhiteSpace(DescriptionText);
		}

		private async Task AnimateCopyButtonAsync()
		{
			string originalText = "复制";
			string successText = "✓ 已复制";

			CopyButtonText = successText;
			await Task.Delay(800);

			CopyButtonText = originalText;
		}

		partial void OnSelectedEventChanged(WorkEvent? value)
		{
			if (value != null)
			{
				TitleText = value.Title;
				DescriptionText = value.Description;
				RemarksText = value.Remarks ?? string.Empty;
				SelectedDate = value.Timestamp.Date;
				SelectedEventType = value.EventType;
				SelectedStatus = value.Status;
			}

			DeleteCommand.NotifyCanExecuteChanged();
			CopyCommand.NotifyCanExecuteChanged();
		}

		partial void OnDescriptionTextChanged(string value)
		{
			CopyCommand.NotifyCanExecuteChanged();
		}

		partial void OnSearchTextChanged(string value)
		{
			_debounceCts?.Cancel();
			_debounceCts?.Dispose();
			_debounceCts = new CancellationTokenSource();

			Task.Delay(330, _debounceCts.Token)
				.ContinueWith(t =>
				{
					if (t.IsCanceled)
					{
						return;
					}

					MainThread.BeginInvokeOnMainThread(FilterEvents);
				}, TaskScheduler.Default);
		}
	}
}
