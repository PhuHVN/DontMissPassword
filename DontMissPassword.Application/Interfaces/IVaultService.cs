using DontMissPassword.Application.DTOs.VaultDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface IVaultService
    {
        Task CreateVault(VaultRequest request);

    }
}
