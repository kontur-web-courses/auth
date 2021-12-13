﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
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
            // NOTE: IHttpContextAccessor позволяет получать HttpContext там, где это не получается сделать более явно.
            var httpContext = httpContextAccessor.HttpContext;
            // NOTE: RouteData содержит информацию о пути и параметрах запроса.
            // Ее сформировал UseRouting и к моменту авторизации уже отработал.
            var routeData = httpContext?.GetRouteData();

            // NOTE: Использовать, если нужное условие выполняется
            // context.Succeed(requirement);

            // NOTE: Использовать, если нужное условие не выполняется
            // context.Fail();

            // NOTE: Этот метод получает информацию о фотографии, в том числе о владельце

            // await photosRepository.GetPhotoMetaAsync(...)
            
            var photoId = routeData.Values["id"]?.ToString();
            if (Guid.TryParse(photoId, out var guid))
            {
                var photoMeta = await photosRepository.GetPhotoMetaAsync(guid);
                if (userId == photoMeta.OwnerId)

                {
                    context.Succeed(requirement);
                    return;
                }
            }

            context.Fail();
        }
    }
}