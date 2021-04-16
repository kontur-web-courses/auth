using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    public class PassportController : Controller
    {
        // NOTE: Неаутентифицированный пользователь будет отправляться на вход в DefaultChallengeScheme,
        // а затем возвращаться сюда и отсюда перенаправляться на исходную страницу из returnUrl.
        public IActionResult Login(string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
                return Challenge();
            
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
