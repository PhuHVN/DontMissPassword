
using DontMissPassword.Application.Interfaces;
using DontMissPassword.Application.Services;
using DontMissPassword.Domain.Abstractions;
using DontMissPassword.Infrastructure.Implemention;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DontMissPassword.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Infrastructure service registrations go here
            services.AddLogging();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IRedisService, OtpCacheService>();
        }

    }
}
