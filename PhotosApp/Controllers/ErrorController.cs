using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace PhotosApp.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet("/Exception")]
        public IActionResult Exception()
        {
            return View();
        }

        [HttpGet("/StatusCode/{code}")]
        [HttpPost("/StatusCode/{code}")]
        public IActionResult StatusCode(HttpStatusCode code)
        {
            return View(code);
        }
    }
}
