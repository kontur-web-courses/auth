using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace PhotosApp.Services.Authorization
{
    using Areas.Identity.Data;

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
            var claimsIdentity = (ClaimsIdentity)principal.Identity;

            if (user.Paid)
            {
                claimsIdentity?.AddClaims(new[]
                {
                    new Claim("subscription ", "paid")
                });
            }

            return principal;
        }
    }
}