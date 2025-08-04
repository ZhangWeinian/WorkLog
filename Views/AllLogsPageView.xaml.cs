using WorkLog.ViewModels;

namespace WorkLog.Views;

public partial class AllLogsPageView : ContentPage
{
	public AllLogsPageView()
	{
		InitializeComponent();

		BindingContext = new AllLogsPageViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is AllLogsPageViewModel vm)
		{
			_ = vm.AppearingCommand.ExecuteAsync(null);
		}
	}
}
