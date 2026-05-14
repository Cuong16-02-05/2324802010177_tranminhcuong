using ASC.DataAccess;
using ASC.Model;
using ASC.Web;
using Microsoft.EntityFrameworkCore;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.PostConfigure<ApplicationSettings>(settings =>
{
    var appSettings = builder.Configuration.GetSection("AppSettings");
    if (appSettings.Exists() && string.IsNullOrEmpty(settings.AdminEmail))
        appSettings.Bind(settings);
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMyDependencyGroup(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // === BƯỚC 1: Migrate DB — tách riêng để biết lỗi rõ ràng ===
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var pendingMigrations = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations: {Migrations}",
                pendingMigrations.Count, string.Join(", ", pendingMigrations));
        }
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully.");

        // Đảm bảo bảng ChatMessages tồn tại cho DB cũ chưa qua migration
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatMessages' AND xtype='U')
            BEGIN
                CREATE TABLE [dbo].[ChatMessages] (
                    [UniqueId]          NVARCHAR(450)  NOT NULL,
                    [ServiceRequestId]  NVARCHAR(450)  NULL,
                    [FromEmail]         NVARCHAR(MAX)  NULL,
                    [FromDisplayName]   NVARCHAR(MAX)  NULL,
                    [ToEmail]           NVARCHAR(MAX)  NULL,
                    [Message]           NVARCHAR(MAX)  NOT NULL DEFAULT '',
                    [SentDate]          DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
                    [IsRead]            BIT            NOT NULL DEFAULT 0,
                    [SenderRole]        NVARCHAR(MAX)  NULL,
                    [CreatedBy]         NVARCHAR(MAX)  NULL,
                    [CreatedDate]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
                    [UpdatedBy]         NVARCHAR(MAX)  NULL,
                    [UpdatedDate]       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
                    [IsDeleted]         BIT            NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([UniqueId])
                );
                CREATE INDEX [IX_ChatMessages_ServiceRequestId]
                    ON [dbo].[ChatMessages] ([ServiceRequestId]);
            END
            IF NOT EXISTS (
                SELECT 1 FROM [__EFMigrationsHistory]
                WHERE [MigrationId] = '20260509000001_AddPriceAndChat'
            )
            INSERT INTO [__EFMigrationsHistory] ([MigrationId],[ProductVersion])
            VALUES ('20260509000001_AddPriceAndChat','8.0.0');
        ");
        logger.LogInformation("ChatMessages table ensured.");
    }
    catch (Exception ex)
    {
        var logger2 = services.GetRequiredService<ILogger<Program>>();
        logger2.LogError(ex, "DATABASE MIGRATION FAILED: {Message}", ex.Message);
        // Không dừng app nhưng log rõ lỗi
    }

    // === BƯỚC 2: Seed data ===
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var options     = services.GetRequiredService<IOptions<ApplicationSettings>>();
        var seed        = services.GetRequiredService<IIdentitySeed>();
        await seed.Seed(userManager, roleManager, options);

        var navCache = services.GetRequiredService<INavigationCacheOperations>();
        await navCache.CreateNavigationCacheAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during startup seed.");
    }
}

app.MapControllerRoute("areaRoute",
    "{area:exists}/{controller=Dashboard}/{action=Dashboard}/{id?}");
app.MapControllerRoute("default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ASC.Web.Hubs.ChatHub>("/chatHub");

app.Run();
