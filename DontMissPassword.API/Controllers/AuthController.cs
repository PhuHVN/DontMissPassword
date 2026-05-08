using DontMissPassword.Application.DTOs;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.AuthDtos;
using DontMissPassword.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DontMissPassword.API.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [SwaggerOperation(summary: "Login with email and password")]        
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var token = await _authService.LoginEmail(request);
            return Ok(ApiResponse<AuthResponse>.OkResponse(token,"Login successful","201"));
        }

        [HttpPost("register")]
        [SwaggerOperation(summary: "Register a new account")]
        public async Task<IActionResult> Register([FromBody] AccountRequest request)
        {
            var result = await _authService.Register(request);
            return Ok(ApiResponse<AccountResponse>.OkResponse(result, "Registration successful", "201"));
        }

        [HttpPost("refresh-token/{refreshToken}")]
        [SwaggerOperation(summary: "Refresh JWT token using a refresh token")]
        public async Task<IActionResult> RefreshToken([FromRoute] string refreshToken)
        {
            var token = await _authService.RefreshToken(refreshToken);
            return Ok(ApiResponse<AuthResponse>.OkResponse(token, "Token refreshed successfully", "200"));
        }
    }
}