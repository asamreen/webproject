using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography; // For SHA256
using System.Text; // For Encoding

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly MongoDbService _dbService;
    private readonly IConfiguration _config;

    public AuthController(MongoDbService dbService, IConfiguration config)
    {
        _dbService = dbService;
        _config = config;
    }

    [HttpPost("local/register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
    {
        if (await _dbService.GetUserByEmailAsync(request.Email) != null)
            return BadRequest(new { message = "Email already exists." });

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbService.CreateUserAsync(user);

        var jwt = JwtHelper.GenerateJwt(user.Id, _config);

        return Ok(new { jwt, user });
    }

    [HttpPost("local")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _dbService.GetUserByEmailAsync(request.Identifier);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var jwt = JwtHelper.GenerateJwt(user.Id, _config);

        return Ok(new { jwt, user });
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
