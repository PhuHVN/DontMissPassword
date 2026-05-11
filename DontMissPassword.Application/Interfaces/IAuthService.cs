using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.AuthDtos;
using DontMissPassword.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginEmail(AuthRequest request);
        Task<Result<string>> Register(AccountRequest request);
        Task<Result> VerifyEmail(string email, string otp);
        Task<Result> ResendOtpAsync(string email);
        Task<Result<string>> RegisterByUsername(AccountRequest request);
        Task<Result<AuthResponse>> RefreshToken(string refreshToken);
    }
}
