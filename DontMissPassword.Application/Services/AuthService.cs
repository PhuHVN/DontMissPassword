using AutoMapper;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.AuthDtos;
using DontMissPassword.Application.DTOs.VaultDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Common.Results;
using DontMissPassword.Domain.Entities;
using DontMissPassword.Domain.Enums;
using System.Text.RegularExpressions;

namespace DontMissPassword.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;
        private readonly IVaultService _vaultService;
        private readonly IEmailService _emailService;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;
        public AuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, IVaultService vaultService, IEmailService emailService, IRedisService redisService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _vaultService = vaultService;
            _emailService = emailService;
            _redisService = redisService;
            _mapper = mapper;
        }
        public async Task<Result<AuthResponse>> LoginEmail(AuthRequest request)
        {
            if (string.IsNullOrEmpty(request.EmailOrUsername) || string.IsNullOrEmpty(request.Password))
            {
                return Result<AuthResponse>.Fail("InvalidInput", "Email and password must be provided.");
            }
            var requestEmail = request.EmailOrUsername.Trim();
            var user = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == requestEmail && x.Status == Domain.Enums.StatusEnum.Active);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Result<AuthResponse>.Fail("Unauthorized", "Invalid email or password.");
            }
            // Generate JWT token and refresh token
            var token = await _jwtProvider.GenerateTokenAsync(user);
            var refreshToken = _jwtProvider.RefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                AccountId = user.Id,
                IsRevoked = false,
                CreateAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),

            };
            await _unitOfWork.GetRepository<RefreshToken>().AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();
            var rs = new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken
            };
            return Result<AuthResponse>.Success(rs);
        }
        public async Task<Result<string>> RegisterByUsername(AccountRequest request)
        {
            if (request.UsernameOrEmail == null || request.Password == null || request.FullName == null)
            {
                return Result<string>.Fail("InvalidInput", "Username, password and full name must be provided.");
            }
            var requestUsername = request.UsernameOrEmail.Trim();
            var existingUser = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == requestUsername);
            if (existingUser != null)
            {
                return Result<string>.Fail("EmailAlreadyInUse", "An account with this email already exists.");
            }
            if (request.Password.Length < 6)
            {
                return Result<string>.Fail("WeakPassword", "Password must be at least 6 characters long.");
            }
            var newUser = new Account
            {
                Email = requestUsername,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Status = Domain.Enums.StatusEnum.Active,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.GetRepository<Account>().AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();
            return Result<string>.Success(requestUsername);
        }
        public async Task<Result<string>> Register(AccountRequest request)
        {
            if (request.UsernameOrEmail == null || request.Password == null || request.FullName == null)
            {
                return Result<string>.Fail("InvalidInput", "Email, password and full name must be provided.");
            }
            var requestEmail = request.UsernameOrEmail.Trim();
            var existingUser = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == requestEmail);
            if (existingUser != null && existingUser.Status == StatusEnum.Active)
            {
                return Result<string>.Fail("EmailAlreadyInUse", "An account with this email already exists.");
            }
            if (existingUser != null && existingUser.Status == StatusEnum.Pending)
            {
                //remove old otp and resend new otp to email
                await _redisService.RemoveOtpAsync(requestEmail);
                var newOtp = _redisService.GenerateOTP();
                await _redisService.StoreOtpAsync(requestEmail, newOtp, TimeSpan.FromMinutes(5));
                await _emailService.SendOtpAsync(requestEmail, newOtp);
                return Result<string>.Success(requestEmail);
            }
            if (existingUser != null && existingUser.Status == StatusEnum.Inactive)
            {
                return Result<string>.Fail("InactiveAccount", "An account with this email is inactive. Please contact support for assistance.");
            }

            //if email not exist, create new account with pending status and send otp to email
            if (!Regex.IsMatch(requestEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return Result<string>.Fail("InvalidEmail", "The email format is invalid.");
            }
            if (request.Password.Length < 6)
            {
                return Result<string>.Fail("WeakPassword", "Password must be at least 6 characters long.");
            }
            var newUser = new Account
            {
                Email = requestEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Status = Domain.Enums.StatusEnum.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var otp = _redisService.GenerateOTP();
            await _unitOfWork.BeginTransactionAsync();
            try
            {

                await _unitOfWork.GetRepository<Account>().AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                await _redisService.StoreOtpAsync(requestEmail, otp, TimeSpan.FromMinutes(5));
                await _emailService.SendOtpAsync(requestEmail, otp);
                return Result<string>.Success(requestEmail);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                return Result<string>.Fail("RegistrationError", "An error occurred while registering the account.");
            }

        }
        public async Task<Result> ResendOtpAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Result.Fail(Error.Invalid);
            }
            var user = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == email && x.Status == Domain.Enums.StatusEnum.Pending);
            if (user == null)
            {
                return Result.Fail(Error.NotFound);
            }
            await _redisService.RemoveOtpAsync(email);
            var otp = _redisService.GenerateOTP();
            await _emailService.SendOtpAsync(email, otp);
            await _redisService.StoreOtpAsync(email, otp, TimeSpan.FromMinutes(5));
            return Result.Success();
        }
        public async Task<Result> VerifyEmail(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return Result.Fail(Error.Invalid);
            }
            var storedOtp = await _redisService.RetrieveOtpAsync(email);
            if (storedOtp == null || storedOtp != otp)
            {
                return Result.Fail(Error.Unauthorized);
            }
            var user = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == email && x.Status == Domain.Enums.StatusEnum.Pending);
            if (user == null)
            {
                return Result.Fail(Error.NotFound);
            }
            user.Status = Domain.Enums.StatusEnum.Active;
            // Create vault for the new user
            await _vaultService.CreateVault(new VaultRequest
            {
                AccountId = user.Id,
            });
            await _unitOfWork.GetRepository<Account>().UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _redisService.RemoveOtpAsync(email);
            return Result.Success();
        }

        public async Task<Result<AuthResponse>> RefreshToken(string refreshToken)
        {
            if (refreshToken == null)
            {
                return Result<AuthResponse>.Fail(Error.Invalid);
            }
            var tokenEntity = await _unitOfWork.GetRepository<RefreshToken>().FindAsync(x => x.Token == refreshToken && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow);
            if (tokenEntity == null)
            {
                return Result<AuthResponse>.Fail(Error.Unauthorized);
            }
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == tokenEntity.AccountId && x.Status == Domain.Enums.StatusEnum.Active);
            if (account == null)
            {
                return Result<AuthResponse>.Fail(Error.Unauthorized);
            }
            // Generate new token and refresh token
            var newToken = await _jwtProvider.GenerateTokenAsync(account);
            var newRefreshToken = _jwtProvider.RefreshToken();
            var newTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                AccountId = account.Id,
                IsRevoked = false,
                CreateAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Revoke the old refresh token
                tokenEntity.IsRevoked = true;
                await _unitOfWork.GetRepository<RefreshToken>().UpdateAsync(tokenEntity);
                // Save the new refresh token
                await _unitOfWork.GetRepository<RefreshToken>().AddAsync(newTokenEntity);
                await _unitOfWork.CommitTransactionAsync();
                return Result<AuthResponse>.Success(new AuthResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception e)
            {
                await _unitOfWork.RollBackAsync();
                throw new Exception("An error occurred while refreshing the token.", e);
            }
        }
    }
}
