using DontMissPassword.Application.DTOs.VaultItemDtos;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IVaultItemService
    {
        Task<Result<ItemResponse>> CreateVaultItem(ItemRequest request);
        Task<Result<ItemResponse>> UpdateVaultItem(string id,ItemRequest request);
        Task<Result> DeleteVaultItem(string id);
        Task<Result<BasePaginatedList<ItemResponse>>> GetVaultItemsByUserLogin(int pageIndex, int pageSize);
        Task<Result<string>> GetPasswordDecrypted(string id);
    }
}
