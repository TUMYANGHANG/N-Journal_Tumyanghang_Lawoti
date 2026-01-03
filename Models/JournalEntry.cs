namespace N_Journal_Tumyanghang_Lawoti.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MarkdownContent { get; set; }
    public DateTime EntryDate { get; set; } // Date only (one entry per day)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int WordCount { get; set; } = 0;

    // Mood Tracking
    public string PrimaryMood { get; set; } = string.Empty; // Required
    public string? SecondaryMood1 { get; set; }
    public string? SecondaryMood2 { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();
}

