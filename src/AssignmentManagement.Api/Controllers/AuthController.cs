using AssignmentManagement.Api.Dtos;
using AssignmentManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AssignmentManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.Login(request);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.Register(request);
        return Ok(result);
    }
}