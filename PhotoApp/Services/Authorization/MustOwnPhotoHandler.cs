using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using PhotoApp.Data;

namespace PhotoApp.Services.Authorization
{
    public class MustOwnPhotoHandler : AuthorizationHandler<MustOwnPhotoRequirement>
    {
        private readonly IPhotoRepository photoRepository;

        public MustOwnPhotoHandler(IPhotoRepository photoRepository)
        {
            this.photoRepository = photoRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MustOwnPhotoRequirement requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            if (filterContext == null)
            {
                context.Fail();
                return;
            }

            var photoIdString = filterContext.RouteData.Values["id"].ToString();

            if (!Guid.TryParse(photoIdString, out Guid photoId))
            {
                context.Fail();
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await photoRepository.IsPhotoOwnerAsync(photoId, userId))
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}