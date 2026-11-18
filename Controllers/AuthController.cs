using Microsoft.AspNetCore.Mvc;
using RevisitsStudiesManager.Back.DTOs;
using RevisitsStudiesManager.Back.Services;

namespace RevisitsStudiesManager.Back.Controllers;

/// <summary>
/// Autenticação: cadastro e login com emissão de token JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
        var user = await _authService.RegisterAsync(request);
        if (user is null)
            return Conflict(new { message = "E-mail já cadastrado." });

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
        var response = await _authService.LoginAsync(request);
        return response is null
            ? Unauthorized(new { message = "E-mail ou senha inválidos." })
            : Ok(response);
    }
}
