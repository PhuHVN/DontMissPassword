using AutoMapper;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Common.Results;
using DontMissPassword.Domain.Entities;
using System.Text.RegularExpressions;

namespace DontMissPassword.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AccountService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<AccountResponse>> CreateAccount(AccountRequest request)
        {
            if (request.Email == null || request.Password == null || request.FullName == null)
            {
                return Result<AccountResponse>.Fail("InvalidInput", "Email, Password and FullName are required.");
            }
            if (Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ArgumentException("Invalid email format.");
            }
            var account = new Account
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName,
                Status = Domain.Enums.StatusEnum.Active
            };
            await _unitOfWork.GetRepository<Account>().AddAsync(account);
            await _unitOfWork.SaveChangesAsync();
            var rs = _mapper.Map<AccountResponse>(account);
            return Result<AccountResponse>.Success(rs);
        }

        public async Task<Result> DeleteAccount(string id)
        {
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == id);
            if (account == null)
            {
                return Result.Fail(Error.NotFound);
            }
            account.Status = Domain.Enums.StatusEnum.Inactive;
            await _unitOfWork.GetRepository<Account>().UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<AccountResponse>> GetAccountById(string id)
        {
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == id);
            if (account == null)
            {
                return Result<AccountResponse>.Fail(Error.NotFound);
            }
            var rs = _mapper.Map<AccountResponse>(account);
            return Result<AccountResponse>.Success(rs);
        }

        public async Task<Result<BasePaginatedList<AccountResponse>>> GetAllAccounts(int pageIndex, int pageSize)
        {
            var query = _unitOfWork.GetRepository<Account>().Entity;
            var rs = await _unitOfWork.GetRepository<Account>().GetPagging(query, pageIndex, pageSize);
            return Result<BasePaginatedList<AccountResponse>>.Success(_mapper.Map<BasePaginatedList<AccountResponse>>(rs));
        }

        public async Task<Result<AccountResponse>> UpdateAccount(AccountRequest account)
        {
            //not implemented yet
            return Result<AccountResponse>.Fail("NotImplemented", "This method is not implemented yet.");
        }
    }
}
