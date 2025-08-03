#if DEBUG

using CommunityToolkit.Maui;

using Microsoft.Extensions.Logging;

#endif


namespace WorkLog
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
					fonts.AddFont("MapleMono-NF-CN-Regular.ttf", "MapleMono");
					fonts.AddFont("Segoe-Fluent-Icons.ttf", "FluentIcons");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
