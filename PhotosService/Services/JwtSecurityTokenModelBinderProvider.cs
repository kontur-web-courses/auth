using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace PhotosService.Services
{
    public class JwtSecurityTokenModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(JwtSecurityToken))
                return new JwtSecurityTokenModelBinder();

            return null;
        }
    }
}
