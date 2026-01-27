using Microsoft.EntityFrameworkCore;
using N_Journal_Tumyanghang_Lawoti.Data;
using N_Journal_Tumyanghang_Lawoti.Models;

namespace N_Journal_Tumyanghang_Lawoti.Services;

public class JournalService
{
    private readonly JournalDbContext _context;

    public JournalService(JournalDbContext context)
        {
            _context = context;
        }

    // Journal Entry Management
    public async Task<JournalEntry?> GetEntryByDateAsync(int userId, DateTime date)
    {
        var entryDate = date.Date;
        return await _context.JournalEntries
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jet => jet.Tag)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.EntryDate.Date == entryDate);
    }

    public async Task<JournalEntry?> GetEntryByIdAsync(int entryId, int userId)
    {
        return await _context.JournalEntries
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jet => jet.Tag)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId);
    }

    public async Task<List<JournalEntry>> GetEntriesAsync(int userId, int skip = 0, int take = 10)
    {
        return await _context.JournalEntries
            .Where(e => e.UserId == userId)
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jet => jet.Tag)
            .OrderByDescending(e => e.EntryDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetEntriesCountAsync(int userId)
    {
        return await _context.JournalEntries
            .Where(e => e.UserId == userId)
            .CountAsync();
    }

        public async Task<JournalEntry> CreateOrUpdateEntryAsync(int userId, DateTime entryDate, string title, 
        string content, string? markdownContent, string primaryMood, string? secondaryMood1, 
        string? secondaryMood2, List<int>? tagIds = null)
    {
        var dateOnly = entryDate.Date;
        var entry = await GetEntryByDateAsync(userId, dateOnly);
        var isNewEntry = entry == null;

        // Prevent creating new entries for any date other than today
        var today = DateTime.UtcNow.Date;
        if (isNewEntry)
        {
            if (dateOnly != today)
            {
                throw new InvalidOperationException("New entries can only be created for today's date. Past or future dates are not allowed.");
            }

            // Double-check: Prevent duplicate entries (should not happen due to unique constraint, but add explicit check)
            var duplicateCheck = await GetEntryByDateAsync(userId, dateOnly);
            if (duplicateCheck != null)
            {
                throw new InvalidOperationException("An entry already exists for this date. You can only create one entry per day.");
            }
        }

        if (entry == null)
        {
            entry = new JournalEntry
            {
                UserId = userId,
                EntryDate = dateOnly,
                CreatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(entry);
        }
        else
        {
            entry.UpdatedAt = DateTime.UtcNow;
        }

        entry.Title = title;
        entry.Content = content;
        entry.MarkdownContent = markdownContent;
        entry.PrimaryMood = primaryMood;
        entry.SecondaryMood1 = secondaryMood1;
        entry.SecondaryMood2 = secondaryMood2;
        entry.WordCount = CountWords(content);

        // Save the entry first to get its Id (especially important for new entries)
        await _context.SaveChangesAsync();

        // Update tags after the entry has been saved and has an Id
        if (tagIds != null && tagIds.Any())
        {
            // Remove existing tags
            var existingTags = _context.JournalEntryTags
                .Where(jet => jet.JournalEntryId == entry.Id)
                .ToList();
            _context.JournalEntryTags.RemoveRange(existingTags);

            // Add new tags
            foreach (var tagId in tagIds)
            {
                _context.JournalEntryTags.Add(new JournalEntryTag
                {
                    JournalEntryId = entry.Id,
                    TagId = tagId
                });
            }
            
            // Save tag changes
            await _context.SaveChangesAsync();
        }
        else if (!isNewEntry)
        {
            // If updating existing entry and no tags provided, remove all tags
            var existingTags = _context.JournalEntryTags
                .Where(jet => jet.JournalEntryId == entry.Id)
                .ToList();
            if (existingTags.Any())
            {
                _context.JournalEntryTags.RemoveRange(existingTags);
                await _context.SaveChangesAsync();
            }
        }

        await UpdateStreakAsync(userId, dateOnly);
        
        return await GetEntryByIdAsync(entry.Id, userId) ?? entry;
    }

    public async Task<bool> DeleteEntryAsync(int entryId, int userId)
    {
        var entry = await GetEntryByIdAsync(entryId, userId);
        if (entry == null) return false;

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync();
        await UpdateStreakAsync(userId, entry.EntryDate);
        return true;
    }

    // Search & Filter
    public async Task<List<JournalEntry>> SearchEntriesAsync(int userId, string? searchTerm = null, 
        DateTime? startDate = null, DateTime? endDate = null, string? mood = null, int? tagId = null, 
        int skip = 0, int take = 10)
    {
        var query = _context.JournalEntries
            .Where(e => e.UserId == userId)
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jet => jet.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Case-insensitive search using EF.Functions.Like (SQLite LIKE is case-insensitive)
            var searchPattern = $"%{searchTerm}%";
            query = query.Where(e => EF.Functions.Like(e.Title, searchPattern) || EF.Functions.Like(e.Content, searchPattern));
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.EntryDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.EntryDate <= endDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(mood))
        {
            query = query.Where(e => e.PrimaryMood == mood || e.SecondaryMood1 == mood || e.SecondaryMood2 == mood);
        }

        if (tagId.HasValue)
        {
            query = query.Where(e => e.JournalEntryTags.Any(jet => jet.TagId == tagId.Value));
        }

        return await query
            .OrderByDescending(e => e.EntryDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

        // Search count (for pagination with filters)
        public async Task<int> SearchEntriesCountAsync(int userId, string? searchTerm = null,
            DateTime? startDate = null, DateTime? endDate = null, string? mood = null, int? tagId = null)
        {
            var query = _context.JournalEntries
                .Where(e => e.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // Case-insensitive search using EF.Functions.Like (SQLite LIKE is case-insensitive)
                var searchPattern = $"%{searchTerm}%";
                query = query.Where(e => EF.Functions.Like(e.Title, searchPattern) || EF.Functions.Like(e.Content, searchPattern));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EntryDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EntryDate <= endDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                query = query.Where(e => e.PrimaryMood == mood || e.SecondaryMood1 == mood || e.SecondaryMood2 == mood);
            }

            if (tagId.HasValue)
            {
                query = query.Where(e => e.JournalEntryTags.Any(jet => jet.TagId == tagId.Value));
            }

            return await query.CountAsync();
        }

    // Tag Management
    public async Task<List<Tag>> GetTagsAsync(int userId, bool includePreBuilt = true)
    {
        var query = _context.Tags.AsQueryable();
        
        if (includePreBuilt)
        {
            query = query.Where(t => t.IsPreBuilt || t.UserId == userId);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<Tag> CreateTagAsync(int userId, string name, string color, bool isPreBuilt = false)
    {
        var tag = new Tag
        {
            Name = name,
            Color = color,
            IsPreBuilt = isPreBuilt,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> DeleteTagAsync(int tagId, int userId)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId && t.UserId == userId && !t.IsPreBuilt);
        if (tag == null) return false;

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    // Streak Management
    public async Task<Streak> GetOrCreateStreakAsync(int userId)
    {
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
        {
            streak = new Streak
            {
                UserId = userId,
                CurrentStreak = 0,
                LongestStreak = 0,
                LastEntryDate = DateTime.MinValue,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Streaks.Add(streak);
            await _context.SaveChangesAsync();
        }
        return streak;
    }

    private async Task UpdateStreakAsync(int userId, DateTime entryDate)
    {
        var streak = await GetOrCreateStreakAsync(userId);
        var today = DateTime.UtcNow.Date;
        var entryDateOnly = entryDate.Date;
        var lastEntryDate = streak.LastEntryDate.Date;

        if (entryDateOnly == today || entryDateOnly == today.AddDays(-1))
        {
            if (lastEntryDate == DateTime.MinValue.Date || lastEntryDate == entryDateOnly.AddDays(-1))
            {
                // Continue streak
                streak.CurrentStreak++;
                if (streak.CurrentStreak > streak.LongestStreak)
                {
                    streak.LongestStreak = streak.CurrentStreak;
                }
            }
            else if (lastEntryDate < entryDateOnly.AddDays(-1))
            {
                // Reset streak
                streak.CurrentStreak = 1;
            }
            // If lastEntryDate == entryDateOnly, it's an update, don't change streak

            streak.LastEntryDate = entryDateOnly;
            streak.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetMissedDaysAsync(int userId)
    {
        var streak = await GetOrCreateStreakAsync(userId);
        if (streak.LastEntryDate == DateTime.MinValue) return 0;

        var today = DateTime.UtcNow.Date;
        var lastEntry = streak.LastEntryDate.Date;
        var daysSinceLastEntry = (today - lastEntry).Days;

        return daysSinceLastEntry > 0 ? daysSinceLastEntry - 1 : 0;
    }

    // Analytics
    public async Task<Dictionary<string, int>> GetMoodDistributionAsync(int userId)
    {
        var entries = await _context.JournalEntries
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var moodCounts = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.PrimaryMood))
            {
                moodCounts[entry.PrimaryMood] = moodCounts.GetValueOrDefault(entry.PrimaryMood, 0) + 1;
            }
            if (!string.IsNullOrEmpty(entry.SecondaryMood1))
            {
                moodCounts[entry.SecondaryMood1] = moodCounts.GetValueOrDefault(entry.SecondaryMood1, 0) + 1;
            }
            if (!string.IsNullOrEmpty(entry.SecondaryMood2))
            {
                moodCounts[entry.SecondaryMood2] = moodCounts.GetValueOrDefault(entry.SecondaryMood2, 0) + 1;
            }
        }

        return moodCounts;
    }

    public async Task<Dictionary<string, int>> GetTagUsageAsync(int userId)
    {
        return await _context.JournalEntryTags
            .Where(jet => jet.JournalEntry.UserId == userId)
            .GroupBy(jet => jet.Tag.Name)
            .Select(g => new { TagName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TagName, x => x.Count);
    }

    public async Task<List<(DateTime Date, int WordCount)>> GetWordCountTrendsAsync(int userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        return await _context.JournalEntries
            .Where(e => e.UserId == userId && e.EntryDate >= startDate)
            .OrderBy(e => e.EntryDate)
            .Select(e => new { e.EntryDate, e.WordCount })
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(e => (e.EntryDate, e.WordCount)).ToList());
    }

    // Export
    public async Task<List<JournalEntry>> GetEntriesForExportAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _context.JournalEntries
            .Where(e => e.UserId == userId && e.EntryDate >= startDate.Date && e.EntryDate <= endDate.Date)
            .Include(e => e.JournalEntryTags)
            .ThenInclude(jet => jet.Tag)
            .OrderBy(e => e.EntryDate)
            .ToListAsync();
    }

    // Helper methods
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

