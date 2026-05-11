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

        [HttpGet]
        public async Task<IActionResult> GetVaultItemsByAccountLogin(int pageIndex = 1, int pageSize = 10)
        {
            var result = await _service.GetVaultItemsByUserLogin(pageIndex, pageSize);
            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<BasePaginatedList<ItemResponse>>.BadRequestResponse(result.Error.Message));
            }
            return Ok(ApiResponse<BasePaginatedList<ItemResponse>>.OkResponse(result.Value, "Get vault items by account login successfully", "200"));
        }
        [HttpGet("decrypt")]
        public async Task<IActionResult> GetDecryptedVaultItemsByAccountLogin(string id)
        {
            var result = await _service.GetPasswordDecrypted(id);
            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<string>.BadRequestResponse(result.Error.Message));
            }

            // Add security headers to prevent caching of sensitive password data
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return Ok(ApiResponse<string>.OkResponse(result.Value, "Get decrypted vault items by account login successfully", "200"));
        }
        [HttpPost]
        public async Task<IActionResult> CreateVaultItem([FromBody] ItemRequest request)
        {
            var result = await _service.CreateVaultItem(request);
            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<ItemResponse>.BadRequestResponse(result.Error.Message));
            }
            return Ok(ApiResponse<ItemResponse>.OkResponse(result.Value, "Vault item created successfully", "201"));
        }
    }

}
