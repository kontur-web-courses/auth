using IdentityServer.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Data
{
    public static class IdentityServerDataExtensions
    {
        public static void PrepareData(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                        userManager.SeedWithSampleUsersAsync().Wait();
                    }
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occurred while migrating or seeding the database.");
                }
            }
        }

        private static async Task SeedWithSampleUsersAsync(this UserManager<ApplicationUser> userManager)
        {
            // NOTE: ToList важен, так как при удалении пользователя меняется список пользователей
            foreach (var user in userManager.Users.ToList())
                await userManager.DeleteAsync(user);

            {
                var user = new ApplicationUser
                {
                    Id = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd",
                    UserName = "vicky@gmail.com",
                    Email = "vicky@gmail.com"
                };
                await userManager.RegisterUserIfNotExists(user, "Pass!2");
                await userManager.AddClaimAsync(user, new Claim("testing", "beta"));
            }

            {
                var user = new ApplicationUser
                {
                    Id = "dcaec9ce-91c9-4105-8d4d-eee3365acd82",
                    UserName = "cristina@gmail.com",
                    Email = "cristina@gmail.com",
                };
                await userManager.RegisterUserIfNotExists(user, "Pass!2");
                await userManager.AddClaimAsync(user, new Claim("subscription", "paid"));
            }

            {
                var user = new ApplicationUser
                {
                    Id = "b9991f69-b4c1-477d-9432-2f7cf6099e02",
                    UserName = "dev@gmail.com",
                    Email = "dev@gmail.com"
                };
                await userManager.RegisterUserIfNotExists(user, "Pass!2");
                await userManager.AddClaimAsync(user, new Claim("subscription", "paid"));
                await userManager.AddClaimAsync(user, new Claim("role", "Dev"));
            }
        }

        private static async Task RegisterUserIfNotExists<TUser>(this UserManager<TUser> userManager,
            TUser user, string password)
            where TUser : IdentityUser<string>
        {
            if (await userManager.FindByNameAsync(user.UserName) == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, code);
                }
            }
        }
    }
}
