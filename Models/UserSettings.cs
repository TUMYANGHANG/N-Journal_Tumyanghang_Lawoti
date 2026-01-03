namespace N_Journal_Tumyanghang_Lawoti.Models;

public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Theme { get; set; } = "light"; // light, dark, or custom
    public string? CustomThemeColors { get; set; } // JSON string for custom theme
    public bool RequirePinForJournal { get; set; } = false;
    public string? JournalPinHash { get; set; } // Hashed PIN for journal protection
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}

