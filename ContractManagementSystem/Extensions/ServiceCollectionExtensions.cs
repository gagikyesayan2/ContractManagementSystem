using ContractManagementSystem.Business.Interfaces;
using ContractManagementSystem.Business.Services;
using ContractManagementSystem.Data.Interfaces;
using ContractManagementSystem.Data.Repositories;



namespace ContractManagementSystem.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
      
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            return services;
        }
    }

}