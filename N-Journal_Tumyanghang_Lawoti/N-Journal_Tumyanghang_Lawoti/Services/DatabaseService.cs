using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Data;

namespace N_Journal_Tumyanghang_Lawoti.Services;

public class DatabaseService
{
    private readonly JournalDbContext _context;

    public DatabaseService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task InitializeDatabaseAsync()
    {
        // Ensure database directory exists
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        var dbDirectory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        // Create database if it doesn't exist
        await _context.Database.EnsureCreatedAsync();
    }
}

