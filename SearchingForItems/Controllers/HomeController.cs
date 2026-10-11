using Microsoft.AspNetCore.Mvc;
using SearchingForItems.Models;
using System.Diagnostics;
using SearchingForItems.Services;


namespace SearchingForItems.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService? api;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApiService? api)
        {
            _logger = logger;
            this.api = api;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/start")]
        public IActionResult Start([FromQuery] int? session, [FromQuery] string? token)
        {
            if (session is > 0)
            {
                HttpContext.Session.SetInt32("game_session_id", session.Value);
                if (!string.IsNullOrWhiteSpace(token))
                    HttpContext.Session.SetString("game_session_token", token);
            }

            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
