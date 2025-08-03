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
		private string _descriptionText = string.Empty;

		[ObservableProperty]
		private string _remarksText = string.Empty;

		[ObservableProperty]
		private DateTime _selectedDate = DateTime.Today;

		[ObservableProperty]
		private WorkEvent? _selectedEvent;

		[ObservableProperty]
		private string _saveButtonText = "保存";

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
				var eventList = await WorkLogDatabase.Instance.GetEventsAsync();
				var sortedEvents = eventList.OrderByDescending(e => e.Timestamp);

				Events.Clear();
				foreach (var ev in sortedEvents)
				{
					Events.Add(ev);
				}
			}
			catch (Exception ex)
			{
				await Shell.Current.DisplayAlert("加载失败", $"加载日志时出错: {ex.Message}", "好的");
			}
		}

		[RelayCommand]
		private async Task SaveAsync()
		{
			if (string.IsNullOrWhiteSpace(DescriptionText))
			{
				await Shell.Current.DisplayAlert("错误", "日志描述不能为空。", "好的");
				return;
			}

			try
			{
				var eventToSave = SelectedEvent ?? new WorkEvent();
				eventToSave.Description = DescriptionText;
				eventToSave.Remarks = RemarksText;
				eventToSave.EventType = SelectedEventType;
				eventToSave.Status = SelectedStatus;

				if (eventToSave.Id == 0)
				{
					DateTime userSelectedDate = SelectedDate.Date;
					TimeSpan currentTime = DateTime.Now.TimeOfDay;
					eventToSave.Timestamp = userSelectedDate.Add(currentTime);
				}

				eventToSave.ProjectId = 1;
				eventToSave.TaskId = 1;

				await WorkLogDatabase.Instance.SaveEventAsync(eventToSave);

				SelectedEvent = null;
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
			await Task.Delay(1500);

			SaveButtonText = originalText;
		}

		[RelayCommand]
		private void ClearForm()
		{
			SelectedEvent = null;
		}

		[RelayCommand(CanExecute = nameof(CanDelete))]
		private async Task DeleteAsync()
		{
			if (SelectedEvent == null)
				return;

			bool answer = await Shell.Current.DisplayAlert("确认删除", $"你确定要删除这条日志吗？\n\n'{SelectedEvent.Description}'", "是，删除", "否");
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

		partial void OnSelectedEventChanged(WorkEvent? value)
		{
			if (value != null)
			{
				DescriptionText = value.Description;
				RemarksText = value.Remarks ?? string.Empty;
				SelectedDate = value.Timestamp.Date;
				SelectedEventType = value.EventType;
				SelectedStatus = value.Status;
			}
			else
			{
				DescriptionText = string.Empty;
				RemarksText = string.Empty;
				SelectedEventType = EventType.Feature;
				SelectedStatus = EventStatus.ToDo;
				SelectedDate = DateTime.Today;
			}

			DeleteCommand.NotifyCanExecuteChanged();
		}
	}
}
