using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManagementSystem.Application.Services;
using TicketManagementSystem.Infrastructure;
using TicketManagementSystem.Infrastructure.Repositories;
using TicketManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

// This file contains extension method to register services in Program.cs
namespace TicketManagementSystem.Api
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            // Application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();

            // Identity stores
            services.AddScoped<IUserStore<ApplicationUser>, SqlUserStore>();
            services.AddScoped<IRoleStore<IdentityRole>, SqlRoleStore>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddDefaultTokenProviders();
        }
    }
}
