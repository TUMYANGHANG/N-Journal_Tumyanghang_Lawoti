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
        // Pre-built tags with color coding
        var preBuiltTags = new[]
        {
            // Work & Career
            new Models.Tag { Name = "Work", Color = "#007bff", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Career", Color = "#0056b3", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Studies", Color = "#6610f2", IsPreBuilt = true, UserId = null },
            
            // Relationships
            new Models.Tag { Name = "Family", Color = "#17a2b8", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Friends", Color = "#6f42c1", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Relationships", Color = "#e83e8c", IsPreBuilt = true, UserId = null },
            
            // Health & Wellness
            new Models.Tag { Name = "Health", Color = "#dc3545", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Fitness", Color = "#fd7e14", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Exercise", Color = "#ff6b6b", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Meditation", Color = "#4ecdc4", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Yoga", Color = "#95e1d3", IsPreBuilt = true, UserId = null },
            
            // Personal Development
            new Models.Tag { Name = "Personal Growth", Color = "#28a745", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Self-care", Color = "#20c997", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Reflection", Color = "#17a2b8", IsPreBuilt = true, UserId = null },
            
            // Activities & Hobbies
            new Models.Tag { Name = "Hobbies", Color = "#ffc107", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Reading", Color = "#6c757d", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Writing", Color = "#495057", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Cooking", Color = "#fd7e14", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Music", Color = "#6f42c1", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Shopping", Color = "#e83e8c", IsPreBuilt = true, UserId = null },
            
            // Travel & Nature
            new Models.Tag { Name = "Travel", Color = "#ffc107", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Nature", Color = "#28a745", IsPreBuilt = true, UserId = null },
            
            // Life Events
            new Models.Tag { Name = "Birthday", Color = "#ff6b6b", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Holiday", Color = "#4ecdc4", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Vacation", Color = "#95e1d3", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Celebration", Color = "#feca57", IsPreBuilt = true, UserId = null },
            
            // Other
            new Models.Tag { Name = "Finance", Color = "#00d2d3", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Spirituality", Color = "#a55eea", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Parenting", Color = "#26de81", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Projects", Color = "#45aaf2", IsPreBuilt = true, UserId = null },
            new Models.Tag { Name = "Planning", Color = "#5f27cd", IsPreBuilt = true, UserId = null }
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

