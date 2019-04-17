using Microsoft.AspNetCore.Authorization;

namespace PhotoApp.Services.Authorization
{
    public class MustOwnPhotoRequirement : IAuthorizationRequirement
    {
        public MustOwnPhotoRequirement()
        {
        }
    }
}