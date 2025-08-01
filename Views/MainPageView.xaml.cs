using WorkLog.ViewModels;

namespace WorkLog;

public partial class MainPageView : ContentPage
{
	public MainPageView()
	{
		InitializeComponent();

		BindingContext = new MainPageViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is MainPageViewModel vm)
		{
			_ = vm.LoadEventsCommand.ExecuteAsync(null);
		}
	}
}
