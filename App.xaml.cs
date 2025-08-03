using System.Diagnostics;

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
			var infoButton = new ImageButton
			{
				Background = Colors.Transparent,
				WidthRequest = 48,
				HeightRequest = 48,
				Padding = new Thickness(15),
				VerticalOptions = LayoutOptions.Center,
				Source = "info.ico",
				Command = new Command(async (url) =>
				{
					try
					{
						if (url is string urlString && !string.IsNullOrEmpty(urlString))
						{
							await Browser.Default.OpenAsync(new Uri(urlString), BrowserLaunchMode.SystemPreferred);
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"无法打开链接: {ex.Message}");
					}
				}),
				CommandParameter = "https://github.com/ZhangWeinian/WorkLog"
			};

			infoButton.Pressed += (sender, args) =>
			{
				(sender as VisualElement)?.ScaleTo(0.9, 100, Easing.CubicOut);
			};
			infoButton.Released += (sender, args) =>
			{
				(sender as VisualElement)?.ScaleTo(1.0, 100, Easing.CubicIn);
			};

			var window = new Window(new AppShell())
			{
				TitleBar = new TitleBar
				{
					HeightRequest = 48,

					Icon = "appicon.ico",

					Title = "牛马 の 日志",
					Subtitle = "Preview",

					TrailingContent = infoButton,
				},
			};

			return window;
		}
	}
}
