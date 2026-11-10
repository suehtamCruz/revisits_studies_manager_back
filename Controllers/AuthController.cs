using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RevisitsStudiesManager.Back.Data;
using RevisitsStudiesManager.Back.DTOs;
using RevisitsStudiesManager.Back.Models;

namespace RevisitsStudiesManager.Back.Controllers;

/// <summary>
/// Autenticação: cadastro e login com emissão de token JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(MongoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Cadastra um novo usuário.
    /// </summary>
    /// <remarks>
    /// A senha é armazenada com hash BCrypt — nunca em texto puro.
    /// </remarks>
    /// <response code="201">Usuário criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">E-mail já cadastrado.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await _context.Users
            .Find(u => u.Email == request.Email)
            .FirstOrDefaultAsync();

        if (existing is not null)
            return Conflict(new { message = "E-mail já cadastrado." });

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _context.Users.InsertOneAsync(user);

        return CreatedAtAction(nameof(Register), new { user.Id, user.Name, user.Email });
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT.
    /// </summary>
    /// <response code="200">Login realizado; retorna o token JWT.</response>
    /// <response code="401">E-mail ou senha inválidos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Find(u => u.Email == request.Email)
            .FirstOrDefaultAsync();

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiryMinutes", 60));
        return Ok(new AuthResponse { Token = GenerateToken(user, expiresAt), ExpiresAt = expiresAt });
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
