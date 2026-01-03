namespace N_Journal_Tumyanghang_Lawoti.Models;

public class JournalEntryTag
{
    public int JournalEntryId { get; set; }
    public int TagId { get; set; }

    // Navigation properties
    public JournalEntry JournalEntry { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

