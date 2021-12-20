using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhotosApp.Data;

namespace PhotosApp.Services.Authorization
{
    public class MustOwnPhotoHandler : AuthorizationHandler<MustOwnPhotoRequirement>
    {
        private readonly IPhotosRepository photosRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MustOwnPhotoHandler(IPhotosRepository photosRepository, IHttpContextAccessor httpContextAccessor)
        {
            this.photosRepository = photosRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MustOwnPhotoRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var httpContext = httpContextAccessor.HttpContext;
            var routeData = httpContext?.GetRouteData();
            var photoIdString = routeData?.Values["id"].ToString();
            if (!Guid.TryParse(photoIdString, out var photoId))
            {
                context.Fail();
                return;
            }
            var photo = await photosRepository.GetPhotoMetaAsync(photoId);
            if (photo != null && photo.OwnerId == userId)
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
        }
    }
}