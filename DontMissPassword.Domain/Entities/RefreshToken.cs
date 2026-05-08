using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public string? ReplacedByToken { get; set; }
        //navigation property
        public Account Account { get; set; } = null!;
        public string AccountId { get; set; } = string.Empty;
    }
}
