using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Models;
using System.Diagnostics;

namespace ProjectManagementApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Projects");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult StatusCode(int code)
        {
            // If the client expects JSON (API / AJAX), return ProblemDetails
            var acceptHeader = Request.Headers["Accept"].ToString();
            if (acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var pd = new ProblemDetails
                {
                    Status = code,
                    Title = code == 404 ? "Not found" :
                            code == 403 ? "Forbidden" : "Error"
                };
                pd.Extensions["requestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                return StatusCode(code, pd);
            }

            // Render an HTML view specific to the status code
            return code switch
            {
                404 => View("NotFound", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }),
                403 => View("Forbidden", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }),
                _ => View("Error")
            };
        }
    }
}
