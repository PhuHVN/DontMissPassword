using DontMissPassword.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IJwtProvider
    {
        Task<string> GenerateTokenAsync(Account acc);
        string RefreshToken();
    }
}
