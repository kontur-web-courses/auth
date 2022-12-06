using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Policy = "Dev")]
    public class DevController : Controller
    {
        public IActionResult Decode()
        {
            return View();
        }
    }
}
