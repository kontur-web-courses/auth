using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    public class PassportController : Controller
    { 
        public IActionResult Login(bool rememberMe, string returnUrl)
        {
            if (!User.Identity.IsAuthenticated)
            {
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