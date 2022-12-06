using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    using System;

    public class PassportController : Controller
    {
        public IActionResult Login(bool rememberMe, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // NOTE: с помощью properties можно задать некоторые параметры будущей сессии.
                // Основные же параметры сессии будут созданы внешним провайдером при обработке Challenge.
                var properties = rememberMe
                    ? new AuthenticationProperties
                    {
                        // NOTE: Кука будет сохраняться при закрытии браузера
                        IsPersistent = true,
                        // NOTE: Кука будет действовать 7 суток
                        ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    }
                    : null;

                return Challenge(properties);
            }

            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }

        [Authorize]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "/"
            }, "Cookie", "Passport");
        }
    }
}