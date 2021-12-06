using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PhotosApp.Areas.Identity.Data;

namespace PhotosApp.Services.Authorization
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<PhotosAppUser, IdentityRole>
    {
        public CustomClaimsPrincipalFactory(
            UserManager<PhotosAppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        public override async Task<ClaimsPrincipal> CreateAsync(PhotosAppUser user)
        {
            var principal = await base.CreateAsync(user);
            if (!user.Paid) return principal;
            var claimsIdentity = (ClaimsIdentity)principal.Identity;

            // NOTE: Вот так можно добавить claim
            claimsIdentity.AddClaims(new[]
            {
                new Claim("subscription", "paid")
            });
            return principal;
        }
    }
}