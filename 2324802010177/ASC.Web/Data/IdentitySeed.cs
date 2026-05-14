using ASC.Model;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASC.Web.Data
{
    public interface IIdentitySeed
    {
        Task Seed(UserManager<ApplicationUser> userManager,
                  RoleManager<IdentityRole> roleManager,
                  IOptions<ApplicationSettings> options);
    }

    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               IOptions<ApplicationSettings> options)
        {
            var s = options.Value;

            // 1. Tạo roles
            string[] roles = { Constants.Roles.Admin, Constants.Roles.Engineer, Constants.Roles.User };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // 2. Tạo Admin
            if (await userManager.FindByEmailAsync(s.AdminEmail!) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = s.AdminEmail,
                    Email = s.AdminEmail,
                    FirstName = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(admin, s.AdminPassword!);
                if (r.Succeeded)
                    await userManager.AddToRoleAsync(admin, Constants.Roles.Admin);
            }

            // 3. Tạo Engineer
            if (await userManager.FindByEmailAsync(s.EngineerEmail!) == null)
            {
                var engineer = new ApplicationUser
                {
                    UserName = s.EngineerEmail,
                    Email = s.EngineerEmail,
                    FirstName = "Engineer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(engineer, s.EngineerPassword!);
                if (r.Succeeded)
                    await userManager.AddToRoleAsync(engineer, Constants.Roles.Engineer);
            }

            // 4. Tạo User
            if (!string.IsNullOrEmpty(s.UserEmail) && await userManager.FindByEmailAsync(s.UserEmail) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = s.UserEmail,
                    Email = s.UserEmail,
                    FirstName = "Customer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var r = await userManager.CreateAsync(user, s.UserPassword!);
                if (r.Succeeded)
                    await userManager.AddToRoleAsync(user, Constants.Roles.User);
            }
        }
    }
}
