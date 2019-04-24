using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhotoApp.Services;

namespace PhotoApp.Controllers
{
    public class HackController : Controller
    {
        [HttpGet("hack/super_secret_qwe123")]
        public IActionResult GenerateToken()
        {
            var encodedJwt = TemporaryTokens.GenerateEncoded();

            Response.Cookies.Append(TemporaryTokens.CookieName, encodedJwt,
                new CookieOptions {HttpOnly = true});
            return Content(encodedJwt);
        }
    }
}