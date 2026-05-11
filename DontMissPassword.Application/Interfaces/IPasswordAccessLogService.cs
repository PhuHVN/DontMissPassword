using DontMissPassword.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IPasswordAccessLogService
    {
        Task<Result> LogPasswordAccess(string vaultItemId, string accountId, string action, string ipAddress = null, string userAgent = null);
        Task<Result<List<object>>> GetPasswordAccessLogs(string vaultItemId, int days = 7);
    }
}
