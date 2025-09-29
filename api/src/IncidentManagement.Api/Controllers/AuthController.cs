using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);
        
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateCredentials(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var isValid = await _authService.ValidateUserCredentialsAsync(loginDto.Email, loginDto.Password);
        return Ok(new { isValid });
    }
}