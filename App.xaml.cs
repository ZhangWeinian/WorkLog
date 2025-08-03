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
			var window = new Window(new AppShell())
			{
				TitleBar = new TitleBar
				{
					HeightRequest = 48,

					Icon = "appicon.ico",

					Title = "牛马 の 日志",
					Subtitle = "Preview",

					Content = new SearchBar
					{
						Placeholder = "全局搜索日志描述或标签...",
						MaximumWidthRequest = 700,
						HorizontalOptions = LayoutOptions.Fill,
						VerticalOptions = LayoutOptions.Center,
					},

					TrailingContent = new ImageButton
					{
						Background = Colors.Transparent,
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
					}
				},
			};

			return window;
		}
	}
}
