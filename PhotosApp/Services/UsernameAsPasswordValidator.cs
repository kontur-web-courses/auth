using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PhotosApp.Services
{
    public class UsernameAsPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : IdentityUser
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            if (string.Equals(user.UserName, password, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "UsernameAsPassword",
                    Description = "Вы не можете использовать имя пользователя в качестве пароля"
                }));
            }
            return Task.FromResult(IdentityResult.Success);
        }
    }
}