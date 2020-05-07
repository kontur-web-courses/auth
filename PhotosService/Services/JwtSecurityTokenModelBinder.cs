using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace PhotosService.Services
{
    public class JwtSecurityTokenModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            if (bindingContext.HttpContext.Items.TryGetValue(typeof(JwtSecurityTokenModelBinder), out var storedValue))
            {
                // NOTE: надеемся, что пришел не просто SecurityToken, а JwtSecurityToken
                bindingContext.Result = ModelBindingResult.Success(storedValue as JwtSecurityToken);
            }
            return Task.CompletedTask;
        }

        public static void SaveToken(HttpContext httpContext, SecurityToken token)
        {
            httpContext.Items[typeof(JwtSecurityTokenModelBinder)] = token;
        }
    }
}
