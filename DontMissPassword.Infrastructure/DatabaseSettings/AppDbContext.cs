using DontMissPassword.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Infrastructure.DatabaseSettings
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        //DbSets 
        public DbSet<Account> Accounts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Vault> Vaults { get; set; }
        public DbSet<VaultItem> VaultItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            // Model configurations go here

            modelBuilder.Entity<VaultItem>()
                .HasIndex(v => v.VaultId);
        }
    }
}
//dotnet ef migrations add AddAccountTable -p DontMissPassword.Infrastructure -s DontMissPassword.API
//dotnet ef database update -p DontMissPassword.Infrastructure -s DontMissPassword.API