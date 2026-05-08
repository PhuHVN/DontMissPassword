using DontMissPassword.Application.DTOs.VaultItemDtos;
using DontMissPassword.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IVaultItemService
    {
        Task<ItemResponse> CreateVaultItem(ItemRequest request);
        Task<ItemResponse> UpdateVaultItem(ItemRequest request);
        Task DeleteVaultItem(string id);
        Task<BasePaginatedList<ItemResponse>> GetAllVaultItems(int pageIndex, int pageSize);
        Task<BasePaginatedList<ItemResponse>> GetVaultItemsByUserLogin(int pageIndex, int pageSize);
        Task<string> GetPasswordDecrypted(string id);
    }
}
