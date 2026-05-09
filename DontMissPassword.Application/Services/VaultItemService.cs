using AutoMapper;
using DontMissPassword.Application.DTOs.VaultItemDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Services
{
    public class VaultItemService : IVaultItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ISecretDataService _secretDataService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public VaultItemService(IUnitOfWork unitOfWork, IUserService userService, ISecretDataService secretDataService, IConfiguration configuration, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _secretDataService = secretDataService;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ItemResponse> CreateVaultItem(ItemRequest request)
        {
            if(request.Username == null || request.Password == null || request.Title == null)
            {
                throw new ArgumentException("Invalid request");
            }
            var user = await _userService.GetUserIdLoginsAsync();
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var vault = await _unitOfWork.GetRepository<Vault>().FindAsync(x => x.AccountId == user.Id );
            if (vault == null)
            {
                throw new ArgumentException("Vault not found");
            }
            var encryptedPassword = _secretDataService.EncryptData(request.Password);
            var vaultItem = new VaultItem
            {
                VaultId = vault.Id,
                Title = request.Title,
                Username = request.Username,
                Password = encryptedPassword.EncryptedData,
                IV = encryptedPassword.IV,
                CreatedAt = DateTime.UtcNow,
                Status = Domain.Enums.StatusEnum.Active
            };
            await _unitOfWork.GetRepository<VaultItem>().AddAsync(vaultItem);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ItemResponse>(vaultItem);

        }

        public async Task DeleteVaultItem(string id)
        {
            var vaultItem = await _unitOfWork.GetRepository<VaultItem>().FindAsync(x => x.Id == id && x.Status == Domain.Enums.StatusEnum.Active);
            if (vaultItem == null)
            {
                throw new ArgumentException("Vault item not found");
            }
            vaultItem.Status = Domain.Enums.StatusEnum.Inactive;
            await _unitOfWork.GetRepository<VaultItem>().UpdateAsync(vaultItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BasePaginatedList<ItemResponse>> GetAllVaultItems(int pageIndex, int pageSize)
        {
            var query = _unitOfWork.GetRepository<VaultItem>().Entity;
            var rs = await _unitOfWork.GetRepository<VaultItem>().GetPagging(query, pageIndex, pageSize);
            return _mapper.Map<BasePaginatedList<ItemResponse>>(rs);
        }

        public async Task<string> GetPasswordDecrypted(string id)
        {
            var user = await _userService.GetUserIdLoginsAsync();
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var vault = await _unitOfWork.GetRepository<Vault>().FindAsync(x => x.AccountId == user.Id);
            if (vault == null)
            {
                throw new ArgumentException("Vault not found");
            }
            var vaultItems = await _unitOfWork.GetRepository<VaultItem>()
                .FindAsync(x => x.VaultId == vault.Id && x.Id == id && x.Status == Domain.Enums.StatusEnum.Active);
            if (vaultItems == null)
            {
                throw new ArgumentException("No vault items found");
            }
            var passwords = _secretDataService.DecryptData(vaultItems.Password, vaultItems.IV);
            return passwords;   
        }

        public async Task<BasePaginatedList<ItemResponse>> GetVaultItemsByUserLogin(int pageIndex, int pageSize)
        {
            var user = await _userService.GetUserIdLoginsAsync();
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var query = _unitOfWork.GetRepository<VaultItem>().Entity.Include(x => x.Vault).Where(x => x.Vault.AccountId == user.Id && x.Status == Domain.Enums.StatusEnum.Active);
            var rs = await _unitOfWork.GetRepository<VaultItem>().GetPagging(query, pageIndex, pageSize);
            return _mapper.Map<BasePaginatedList<ItemResponse>>(rs);
        }

        public Task<ItemResponse> UpdateVaultItem(ItemRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
