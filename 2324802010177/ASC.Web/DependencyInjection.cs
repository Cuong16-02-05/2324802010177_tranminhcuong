using ASC.Business;
using ASC.DataAccess;
using ASC.Model;
using ASC.Web.Data;
using ASC.Web.Infrastructure;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMyDependencyGroup(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ASC.DataAccess")));

            services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Google OAuth (optional)
            var googleClientId = configuration["Authentication:Google:ClientId"];
            var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrEmpty(googleClientId) && googleClientId != "YOUR_GOOGLE_CLIENT_ID")
            {
                services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret!;
                });
            }

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // Data Access
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Business Layer
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddScoped<IChatOperations, ChatOperations>();

            // SignalR (real-time chat)
            services.AddSignalR();

            // AutoMapper
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            // Infrastructure
            services.AddScoped<IIdentitySeed, IdentitySeed>();
            services.AddTransient<IEmailSender, AuthMessageSender>();

            // DI Lifetime Demo
            services.AddTransient<TransientLoggerService>();
            services.AddScoped<ScopedLoggerService>();
            services.AddSingleton<SingletonLoggerService>();

            // Cache & Navigation
            services.AddMemoryCache();
            services.AddScoped<INavigationCacheOperations, NavigationCacheOperations>();

            // Session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return services;
        }
    }
}
