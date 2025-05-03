using BookingSystem.Application.DTOs;
using BookingSystem.Application.DTOs.User;
using BookingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterUserDto registerDto)
    {
        try
        {
            var response = await _authService.Register(registerDto);
            
            // Store JWT token in HTTP-only cookie
            SetTokenCookie(response.Token);
            
            // response.Token = null;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
    {
        try
        {
            var response = await _authService.Login(loginDto);
            
            // Store JWT token in HTTP-only cookie
            SetTokenCookie(response.Token);
            
            // response.Token = null;
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the authentication cookie
        Response.Cookies.Delete("X-Access-Token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
        
        return Ok(new { message = "Logged out successfully" });
    }
    
    private void SetTokenCookie(string token)
    {
        // Get token expiration from configuration (default 7 days)
        var expirationDays = _configuration.GetValue<int>("JwtSettings:DurationInDays", 7);
        
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents client-side JS from reading the cookie
            Expires = DateTime.UtcNow.AddDays(expirationDays),
            Secure = true, // Requires HTTPS
            SameSite = SameSiteMode.Strict, // Prevents CSRF
            Path = "/" // Available across the entire site
        };
        
        Response.Cookies.Append("X-Access-Token", token, cookieOptions);
    }
}