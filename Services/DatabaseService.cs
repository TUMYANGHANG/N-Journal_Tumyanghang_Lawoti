using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Data;
using N_Journal_Tumyanghang_Lawoti.Models;

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

        // Seed pre-built tags if they don't exist
        await SeedPreBuiltTagsAsync();
    }

    private async Task SeedPreBuiltTagsAsync()
    {
        var preBuiltTags = new[]
        {
            new Models.Tag { Name = "Work", Color = "#007bff", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Personal", Color = "#28a745", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Health", Color = "#dc3545", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Travel", Color = "#ffc107", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Family", Color = "#17a2b8", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Friends", Color = "#6f42c1", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Goals", Color = "#e83e8c", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Reflection", Color = "#20c997", IsPreBuilt = true, UserId = null }
        };

        foreach (var tag in preBuiltTags)
        {
            var exists = await _context.Tags.AnyAsync(t => t.Name == tag.Name && t.IsPreBuilt && t.UserId == null);
            if (!exists)
            {
                tag.CreatedAt = DateTime.UtcNow;
                _context.Tags.Add(tag);
            }
        }

        await _context.SaveChangesAsync();
    }
}

