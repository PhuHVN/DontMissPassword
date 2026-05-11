using System;
using System.ComponentModel.DataAnnotations;

namespace DontMissPassword.Domain.Entities
{
    public class PasswordAccessLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string VaultItemId { get; set; }

        [Required]
        public string AccountId { get; set; }

        [Required]
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public string Action { get; set; } // "VIEWED", "COPIED", "AUTO_CLEARED"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
