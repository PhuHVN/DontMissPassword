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
            return Ok(ApiResponse<AuthResponse>.OkResponse(token.Value,"Login successful","201"));
        }

        [HttpPost("register")]
        [SwaggerOperation(summary: "Register a new account")]
        public async Task<IActionResult> Register([FromBody] AccountRequest request)
        {
            var result = await _authService.Register(request);
            return Ok(ApiResponse<string>.OkResponse(request.Email,"Registration successful please check email", "201")); 
        }

        [HttpPatch("verifyOtp")]
        [SwaggerOperation(summary: "Verify the user's email using OTP")]
        public async Task<IActionResult> VerifyEmail(VerifyOtpDtos verifyOtp)
        {
            await _authService.VerifyEmail(verifyOtp.Email, verifyOtp.Otp);
            return Ok(ApiResponse<string>.OkResponse(null, "Email verified successfully", "200"));
        }

        [HttpPost("resendOtp/{email}")]
        [SwaggerOperation(summary: "Resend OTP to the user's email")]
        public async Task<IActionResult> ResendOtp([FromRoute] string email)
        {
            await _authService.ResendOtpAsync(email);
            return Ok(ApiResponse<string>.OkResponse(null, "OTP resent successfully", "200"));
        }

        [HttpPost("refresh-token/{refreshToken}")]
        [SwaggerOperation(summary: "Refresh JWT token using a refresh token")]
        public async Task<IActionResult> RefreshToken([FromRoute] string refreshToken)
        {
            var token = await _authService.RefreshToken(refreshToken);
            return Ok(ApiResponse<AuthResponse>.OkResponse(token.Value, "Token refreshed successfully", "200"));
        }
    }
}