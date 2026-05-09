using AutoMapper;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.DTOs.AuthDtos;
using DontMissPassword.Application.DTOs.VaultDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Entities;
using System.Text.RegularExpressions;

namespace DontMissPassword.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;
        private readonly IVaultService _vaultService;
        private readonly IMapper _mapper;
        public AuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, IVaultService vaultService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _vaultService = vaultService;
            _mapper = mapper;
        }
        public async Task<AuthResponse> LoginEmail(AuthRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Email and password must be provided.");
            }
            var requestEmail = request.Email.Trim().ToLower();
            var user = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == requestEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
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
            return new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<AccountResponse> Register(AccountRequest request)
        {
            if (request.Email == null || request.Password == null || request.FullName == null)
            {
                throw new ArgumentException("Email, password and full name must be provided.");
            }
            var requestEmail = request.Email.Trim().ToLower();
            var existingUser = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Email == requestEmail && x.Status == Domain.Enums.StatusEnum.Active);
            if (existingUser != null)
            {
                throw new ArgumentException("Email is already in use.");
            }
            if (!Regex.IsMatch(requestEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ArgumentException("Invalid email format.");
            }
            if (request.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long.");
            }
            var newUser = new Account
            {
                Email = requestEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Status = Domain.Enums.StatusEnum.Active,
                CreatedAt = DateTime.UtcNow
            };
            var vaultRequest = new VaultRequest
            {
                AccountId = newUser.Id,
            };
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                
                
                await _unitOfWork.GetRepository<Account>().AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();
                // Create vault for the new user
                await _vaultService.CreateVault(vaultRequest);
                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<AccountResponse>(newUser);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollBackAsync();
                throw new ArgumentException("An error occurred while registering the account.", ex);
            }

        }

        public async Task<AuthResponse> RefreshToken(string refreshToken)
        {
            if (refreshToken == null)
            {
                throw new ArgumentNullException("Refresh token must be provided.");
            }
            var tokenEntity = await _unitOfWork.GetRepository<RefreshToken>().FindAsync(x => x.Token == refreshToken && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow);
            if (tokenEntity == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == tokenEntity.AccountId && x.Status == Domain.Enums.StatusEnum.Active);
            if (account == null)
            {
                throw new UnauthorizedAccessException("Account not found or inactive.");
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
                return new AuthResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception e)
            {
                await _unitOfWork.RollBackAsync();
                throw new Exception("An error occurred while refreshing the token.", e);
            }
        }
    }
}
