using DontMissPassword.Application.DTOs;
using DontMissPassword.Application.DTOs.VaultItemDtos;
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DontMissPassword.API.Controllers
{
    [Route("api/v1/vault-items")]
    [ApiController]
    [Authorize]
    public class VaultItemController : ControllerBase
    {
        private readonly IVaultItemService _service;

        public VaultItemController(IVaultItemService service)
        {
            _service = service;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllVaultItems(int pageIndex = 1, int pageSize = 10)
        {
            var result = await _service.GetAllVaultItems(pageIndex, pageSize);
            return Ok(ApiResponse<BasePaginatedList<ItemResponse>>.OkResponse(result, "Get all vault items successfully", "200"));
        }
        [HttpGet]
        public async Task<IActionResult> GetVaultItemsByAccountLogin(int pageIndex = 1, int pageSize = 10)
        {
            var result = await _service.GetVaultItemsByUserLogin(pageIndex, pageSize);
            return Ok(ApiResponse<BasePaginatedList<ItemResponse>>.OkResponse(result, "Get vault items by account login successfully", "200"));
        }
        [HttpGet("decrypt")]
        public async Task<IActionResult> GetDecryptedVaultItemsByAccountLogin(string id)
        {
            var result = await _service.GetPasswordDecrypted(id);
            return Ok(ApiResponse<string>.OkResponse(result, "Get decrypted vault items by account login successfully", "200"));
        }
        [HttpPost]
        public async Task<IActionResult> CreateVaultItem([FromBody] ItemRequest request)
        {
            var result = await _service.CreateVaultItem(request);
            return Ok(ApiResponse<ItemResponse>.OkResponse(result, "Vault item created successfully", "201"));
        }
    }

}
