using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Data;
using N_Journal_Tumyanghang_Lawoti.Services;

namespace N_Journal_Tumyanghang_Lawoti;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Register database context
		builder.Services.AddDbContext<JournalDbContext>(options =>
		{
			// Use FileSystem.AppDataDirectory for MAUI - stores in app's data directory
			var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
			options.UseSqlite($"Data Source={dbPath}");
		});

		// Register services
		builder.Services.AddScoped<DatabaseService>();
		// Use Scoped for AuthService (works with DbContext)
		// Authentication state is persisted using Preferences
		builder.Services.AddScoped<AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Initialize database on startup
		using (var scope = app.Services.CreateScope())
		{
			var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
			_ = Task.Run(async () => await dbService.InitializeDatabaseAsync());
		}

		return app;
	}
}
