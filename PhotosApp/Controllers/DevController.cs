using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    [Authorize(Policy = "Dev")]
    public class DevController : Controller
    {
        public IActionResult Decode()
        {
            return View();
        }
    }
}
