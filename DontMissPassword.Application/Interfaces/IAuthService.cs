using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.AuthDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginEmail(AuthRequest request);
        Task<AccountResponse> Register(AccountRequest request);
        Task VerifyEmail(string email, string otp);
        Task ResendOtpAsync(string email);
        Task<AuthResponse> RefreshToken(string refreshToken);
    }
}
