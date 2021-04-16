using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    public class PassportController : Controller
    {
        // NOTE: Неаутентифицированный пользователь будет отправляться на вход в DefaultChallengeScheme,
        // а затем возвращаться сюда и отсюда перенаправляться на исходную страницу из returnUrl.
        public IActionResult Login(bool rememberMe, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // NOTE: с помощью properties можно задать некоторые параметры будущей сессии.
                // Основные же параметры сессии будут созданы внешним провайдером при обработке Challenge.
                var properties = rememberMe
                    ? new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(7),
                        }
                    : null;

                return Challenge(properties);
            }
            
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }

        // NOTE: Выход из текущей схемы аутентификации с последующей переадресацией
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
