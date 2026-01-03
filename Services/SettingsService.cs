using Microsoft.EntityFrameworkCore;
using N_Journal_Tumyanghang_Lawoti.Data;
using N_Journal_Tumyanghang_Lawoti.Models;
using BCrypt.Net;

namespace N_Journal_Tumyanghang_Lawoti.Services;

public class SettingsService
{
    private readonly JournalDbContext _context;

    public SettingsService(JournalDbContext context)
    {
        _context = context;
    }

    public async Task<UserSettings> GetOrCreateSettingsAsync(int userId)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                Theme = "light",
                RequirePinForJournal = false,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task UpdateThemeAsync(int userId, string theme, string? customThemeColors = null)
    {
        var settings = await GetOrCreateSettingsAsync(userId);
        settings.Theme = theme;
        settings.CustomThemeColors = customThemeColors;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SetJournalPinAsync(int userId, string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4) return false;

        var settings = await GetOrCreateSettingsAsync(userId);
        settings.JournalPinHash = BCrypt.Net.BCrypt.HashPassword(pin);
        settings.RequirePinForJournal = true;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyJournalPinAsync(int userId, string pin)
    {
        var settings = await GetOrCreateSettingsAsync(userId);
        if (!settings.RequirePinForJournal || string.IsNullOrEmpty(settings.JournalPinHash))
            return true; // No PIN required

        return BCrypt.Net.BCrypt.Verify(pin, settings.JournalPinHash);
    }

    public async Task DisableJournalPinAsync(int userId)
    {
        var settings = await GetOrCreateSettingsAsync(userId);
        settings.RequirePinForJournal = false;
        settings.JournalPinHash = null;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}

