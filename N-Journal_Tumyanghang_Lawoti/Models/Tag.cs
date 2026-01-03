namespace N_Journal_Tumyanghang_Lawoti.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d"; // Default gray color
    public bool IsPreBuilt { get; set; } = false; // Pre-built tags vs custom tags
    public int? UserId { get; set; } // Nullable for pre-built tags, or specific user for custom tags
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();
}

