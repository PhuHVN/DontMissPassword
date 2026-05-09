using DontMissPassword.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DontMissPassword.Domain.Entities
{
    public class Account
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Active;
        // Navigation property 
        public ICollection<RefreshToken> RefreshToken { get; set; } = new List<RefreshToken>();
        
    }
}
