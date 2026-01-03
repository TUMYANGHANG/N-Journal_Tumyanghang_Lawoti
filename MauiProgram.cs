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

		// Register database context with exception handling
		builder.Services.AddDbContext<JournalDbContext>(options =>
		{
			try
			{
				var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
				options.UseSqlite($"Data Source={dbPath}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Database configuration error: {ex}");
			}
		});

		// Register services
		builder.Services.AddScoped<DatabaseService>();
		builder.Services.AddScoped<AuthService>();
		builder.Services.AddScoped<JournalService>();
		builder.Services.AddScoped<SettingsService>();
		builder.Services.AddScoped<PdfExportService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Initialize database on startup (fire and forget, but will complete before first use)
		Task.Run(async () =>
		{
			try
			{
				await Task.Delay(500); // Small delay to ensure app is fully loaded
				using var scope = app.Services.CreateScope();
				var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
				await dbService.InitializeDatabaseAsync();
				System.Diagnostics.Debug.WriteLine("Database initialized successfully");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			}
		});

		return app;
	}
}
