using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Common.Results;
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
        Task<Result<AccountResponse>> CreateAccount(AccountRequest request);
        Task<Result<AccountResponse>> UpdateAccount(AccountRequest request);
        Task<Result<AccountResponse>> GetAccountById(string id);
        Task<Result<BasePaginatedList<AccountResponse>>> GetAllAccounts(int pageIndex, int pageSize);
        Task<Result> DeleteAccount(string id);
    }
}
