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
            ViewBag.PortalUserName = HttpContext.Session.GetString("portal_user_name");
            ViewBag.PortalTotalPoints = HttpContext.Session.GetInt32("portal_total_points");
            return View();
        }

        [HttpGet("/start")]
        public async Task<IActionResult> Start([FromQuery] int? session, [FromQuery] string? token)
        {
            if (!ModelState.IsValid || session is not > 0 || string.IsNullOrWhiteSpace(token))
                return Unauthorized("Портал не передал корректную игровую сессию и токен");

            try
            {
                var portalSession = await api!.ResolveGameSessionAsync(session.Value, token);
                if (portalSession is not null)
                {
                    HttpContext.Session.SetInt32("game_session_id", session.Value);
                    HttpContext.Session.SetString("game_session_token", token);
                    HttpContext.Session.SetString("portal_user_id", portalSession.userId);
                    HttpContext.Session.SetString("portal_user_name", portalSession.userName);
                    HttpContext.Session.SetInt32("portal_total_points", portalSession.totalUserPoints);
                    ViewBag.PortalUserName = portalSession.userName;
                    ViewBag.PortalTotalPoints = portalSession.totalUserPoints;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Портал отклонил игровую сессию {GameSessionId}", session.Value);
                return Unauthorized("Игровая сессия отклонена порталом");
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
