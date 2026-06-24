using Microsoft.Extensions.DependencyInjection;
using TicketManagementSystem.Application.Services;
using TicketManagementSystem.Infrastructure.Repositories;
using TicketManagementSystem.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using TicketManagementSystem.Infrastructure.Identity;

namespace TicketManagementSystem.Api
{
    public static partial class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();

            // Application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<ICancellationService, CancellationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IReportService, ReportService>();

            // Identity stores
            services.AddScoped<IUserStore<ApplicationUser>, SqlUserStore>();
            services.AddScoped<IRoleStore<IdentityRole>, SqlRoleStore>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddDefaultTokenProviders();

            // Authorization - dynamic permission policies
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        }
    }
}
