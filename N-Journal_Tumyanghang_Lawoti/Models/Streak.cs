namespace N_Journal_Tumyanghang_Lawoti.Models;

public class Streak
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime LastEntryDate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
}

