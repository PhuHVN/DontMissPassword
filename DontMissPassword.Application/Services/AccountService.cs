using AutoMapper;
using DontMissPassword.Application.DTOs.AccountDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
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

        public async Task<AccountResponse> CreateAccount(AccountRequest request)
        {
            if (request.Email == null || request.Password == null || request.FullName == null)
            {
                throw new ArgumentException("Email, Password and FullName are required.");
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
            return _mapper.Map<AccountResponse>(account);
        }

        public async Task DeleteAccount(string id)
        {
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == id);
            if (account == null)
            {
                throw new ArgumentException("Account not found.");
            }
            account.Status = Domain.Enums.StatusEnum.Inactive;
            await _unitOfWork.GetRepository<Account>().UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<AccountResponse> GetAccountById(string id)
        {
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == id);
            return _mapper.Map<AccountResponse>(account);
        }

        public async Task<BasePaginatedList<AccountResponse>> GetAllAccounts(int pageIndex, int pageSize)
        {
            var query = _unitOfWork.GetRepository<Account>().Entity;
            var rs = await _unitOfWork.GetRepository<Account>().GetPagging(query, pageIndex, pageSize);
            return _mapper.Map<BasePaginatedList<AccountResponse>>(rs);
        }

        public Task<AccountResponse> UpdateAccount(AccountRequest account)
        {
            throw new NotImplementedException();
        }
    }
}
