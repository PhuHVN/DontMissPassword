using DontMissPassword.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.DTOs.VaultItemDtos
{
    public class ItemResponse
    {
        public string Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Active;
        public string VaultId { get; set; } = string.Empty;
    }
}
