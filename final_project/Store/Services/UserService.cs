using MongoDB.Driver;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography; // For SHA256
using System.Text; // For Encoding

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(config["MongoDB:Database"]);
        _users = database.GetCollection<User>("Users");
    }

    public async Task SeedGuestUserAsync()
    {
        var guestEmail = "test@gmail.com";

        // Check if the guest user already exists
        var existingUser = await _users.Find(u => u.Email == guestEmail).FirstOrDefaultAsync();
        if (existingUser != null)
        {

            Console.WriteLine("Guest user already exists.");
            return;
        }

        // Create the guest user
        var guestUser = new User
        {
            Username="Guest",
            Email = guestEmail,
            PasswordHash = HashPassword("secret"), // Secure password hashing
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _users.InsertOneAsync(guestUser);
        Console.WriteLine("Guest user seeded successfully.");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }
}
