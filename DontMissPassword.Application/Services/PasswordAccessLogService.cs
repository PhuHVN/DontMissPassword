using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Common.Results;
using DontMissPassword.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Services
{
    public class PasswordAccessLogService : IPasswordAccessLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PasswordAccessLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> LogPasswordAccess(string vaultItemId, string accountId, string action, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var log = new PasswordAccessLog
                {
                    VaultItemId = vaultItemId,
                    AccountId = accountId,
                    Action = action,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AccessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetRepository<PasswordAccessLog>().AddAsync(log);
                await _unitOfWork.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(Error.Conflict);
            }
        }

        public async Task<Result<List<object>>> GetPasswordAccessLogs(string vaultItemId, int days = 7)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                var logs = await _unitOfWork.GetRepository<PasswordAccessLog>()
                    .FilterByAsync(x => x.VaultItemId == vaultItemId && x.AccessedAt >= cutoffDate);

                var result = logs
                    .OrderByDescending(x => x.AccessedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.Action,
                        x.AccessedAt,
                        x.IpAddress,
                        UserAgentPreview = x.UserAgent?.Length > 50 ? x.UserAgent.Substring(0, 50) + "..." : x.UserAgent
                    })
                    .Cast<object>()
                    .ToList();

                return Result<List<object>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<object>>.Fail("LogError", $"Failed to retrieve access logs: {ex.Message}");
            }
        }
    }
}
