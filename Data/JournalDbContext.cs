using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Models;

namespace N_Journal_Tumyanghang_Lawoti.Data;

public class JournalDbContext : DbContext
{

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     // Database will be stored in the app's data directory
    //     // This is configured in MauiProgram.cs, but we provide a fallback here
    //     if (!optionsBuilder.IsConfigured)
    //     {
    //         var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
    //         optionsBuilder.UseSqlite($"Data Source={dbPath}");
    //     }
    // }
     public JournalDbContext(DbContextOptions<JournalDbContext> options)
            : base(options)
        {
        }


    public DbSet<User> Users { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<JournalEntryTag> JournalEntryTags { get; set; }
    public DbSet<Streak> Streaks { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.EntryDate }).IsUnique(); // One entry per day per user
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.PrimaryMood).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SecondaryMood1).HasMaxLength(50);
            entity.Property(e => e.SecondaryMood2).HasMaxLength(50);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.UserId });
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#6c757d");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        // JournalEntryTag (many-to-many) configuration
        modelBuilder.Entity<JournalEntryTag>(entity =>
        {
            entity.HasKey(e => new { e.JournalEntryId, e.TagId });
            entity.HasOne(e => e.JournalEntry)
                .WithMany(j => j.JournalEntryTags)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag)
                .WithMany(t => t.JournalEntryTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Streak configuration
        modelBuilder.Entity<Streak>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserSettings configuration
        modelBuilder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Theme).HasMaxLength(20).HasDefaultValue("light");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

