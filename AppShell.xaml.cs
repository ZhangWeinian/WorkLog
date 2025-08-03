using WorkLog.Views;

namespace WorkLog
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();

			Routing.RegisterRoute(nameof(AllLogsPageView), typeof(AllLogsPageView));
		}
	}
}
