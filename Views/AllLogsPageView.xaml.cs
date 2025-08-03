using WorkLog.ViewModels;

namespace WorkLog.Views;

public partial class AllLogsPageView : ContentPage
{
	public AllLogsPageView()
	{
		InitializeComponent();

		BindingContext = new AllLogsPageViewModel();
	}
}
