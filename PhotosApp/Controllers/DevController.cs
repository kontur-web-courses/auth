using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    [Authorize(Roles = "Dev", AuthenticationSchemes = "Bearer, Identity.Application")]
    public class DevController : Controller
    {
        public IActionResult Decode()
        {
            return View();
        }
    }
}
