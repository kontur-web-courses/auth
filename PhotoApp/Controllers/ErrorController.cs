using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace PhotoApp.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet("/Error/StatusCode/500")]
        public IActionResult InternalServerError()
        {
            return View();
        }

        [HttpGet("/Error/StatusCode/{code}")]
        public IActionResult StatusCode(HttpStatusCode code)
        {
            return View(code);
        }
    }
}
