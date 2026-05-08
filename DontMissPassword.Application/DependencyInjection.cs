using DontMissPassword.Application.Interfaces;
using DontMissPassword.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DontMissPassword.Application
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Application service registrations go here
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVaultService, VaultService>();
            services.AddScoped<IVaultItemService, VaultItemService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISecretDataService, SecretDataService>();
        }


    }
}
