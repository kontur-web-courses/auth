using Microsoft.AspNetCore.Authorization;

namespace PhotosApp.Services.Authorization
{
    public class MustOwnPhotoRequirement : IAuthorizationRequirement
    {
        public MustOwnPhotoRequirement()
        {
        }
    }
}