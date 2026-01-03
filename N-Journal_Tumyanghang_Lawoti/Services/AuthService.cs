using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using N_Journal_Tumyanghang_Lawoti.Data;
using N_Journal_Tumyanghang_Lawoti.Models;
using BCrypt.Net;

namespace N_Journal_Tumyanghang_Lawoti.Services;

public class AuthService
{
    private readonly JournalDbContext _context;
    private User? _currentUser;
    private const string CurrentUserIdKey = "CurrentUserId";

    public AuthService(JournalDbContext context)
    {
        _context = context;
        _ = LoadCurrentUserAsync();
    }

    public User? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;

    private async Task LoadCurrentUserAsync()
    {
        try
        {
            var userId = Preferences.Get(CurrentUserIdKey, -1);
            if (userId > 0)
            {
                _currentUser = await _context.Users.FindAsync(userId);
            }
        }
        catch
        {
            // Ignore errors during initialization
        }
    }

    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        try
        {
            // Ensure database is initialized
            await _context.Database.EnsureCreatedAsync();

            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return false;
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user == null)
            {
                return false;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }

            _currentUser = user;
            Preferences.Set(CurrentUserIdKey, user.Id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        _currentUser = null;
        Preferences.Remove(CurrentUserIdKey);
    }
}

