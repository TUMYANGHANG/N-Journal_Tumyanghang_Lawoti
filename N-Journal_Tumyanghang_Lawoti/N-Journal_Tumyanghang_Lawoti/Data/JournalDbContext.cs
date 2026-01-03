using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Models;

namespace N_Journal_Tumyanghang_Lawoti.Data;

public class JournalDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Database will be stored in the app's data directory
        // This is configured in MauiProgram.cs, but we provide a fallback here
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
        });
    }
}

