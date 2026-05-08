using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Domain.Entities
{
    public class Vault
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        // Navigation property
        public string AccountId { get; set; } = string.Empty;
        public Account Account { get; set; } = null!;
        public ICollection<VaultItem> VaultItems { get; set; } = new List<VaultItem>();

    }
}
