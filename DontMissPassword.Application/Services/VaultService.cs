using DontMissPassword.Application.DTOs.VaultDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Entities;
using DontMissPassword.Domain.Enums;

namespace DontMissPassword.Application.Services
{
    public class VaultService : IVaultService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaultService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateVault(VaultRequest request)
        {
            if (string.IsNullOrEmpty(request.AccountId))
            {
                throw new ArgumentNullException(nameof(request.AccountId));
            }
            var account = await _unitOfWork.GetRepository<Account>().FindAsync(x => x.Id == request.AccountId && x.Status == StatusEnum.Active);
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }
            var existingVault = await _unitOfWork.GetRepository<Vault>().FindAsync(x => x.AccountId == request.AccountId);
            // return if vault already exists for the account, as each account can have only one vault
            if (existingVault != null)
            {
                return;
            }
            var vault = new Vault
            {
                Id = Guid.NewGuid().ToString(),
                AccountId = request.AccountId,
            };
            await _unitOfWork.GetRepository<Vault>().AddAsync(vault);
            await _unitOfWork.SaveChangesAsync();
        }


    }
}
