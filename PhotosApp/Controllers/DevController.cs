using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    public class DevController : Controller
    {
        [Authorize(Policy = "Dev")]
        public IActionResult Decode()
        {
            return View();
        }
    }
}
