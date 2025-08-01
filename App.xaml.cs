using WorkLog.Services;

namespace WorkLog
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
			_ = WorkLogDatabase.Instance.Init();
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
			return new Window(new AppShell());
		}
	}
}
