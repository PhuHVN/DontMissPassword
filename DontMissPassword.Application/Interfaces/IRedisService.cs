using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IRedisService
    {
        string GenerateOTP();
        Task RemoveOtpAsync(string key);
        Task<string?> RetrieveOtpAsync(string key);
        Task StoreOtpAsync(string key, string otp, TimeSpan expiration);
    }
}
