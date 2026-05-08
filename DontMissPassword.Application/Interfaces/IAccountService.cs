using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountResponse> CreateAccount(AccountRequest request);
        Task<AccountResponse> UpdateAccount(AccountRequest request);
        Task<AccountResponse> GetAccountById(string id);
        Task<BasePaginatedList<AccountResponse>> GetAllAccounts(int pageIndex, int pageSize);
        Task DeleteAccount(string id);
    }
}
