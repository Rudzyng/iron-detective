using Microsoft.AspNetCore.Mvc;
using SearchingForItems.Data;
using SearchingForItems.Models;
using SearchingForItems.Services;

namespace SearchingForItems.Controllers
{
    public class GameController : Controller
    {
        private LocationRepository _locationRepository;
        private UserRepository _userRepository;
        private readonly ApiService? api;

        public GameController(LocationRepository locationRepository, UserRepository userRepository, ApiService? api)
        {
            _locationRepository = locationRepository;
            _userRepository = userRepository;
            this.api = api;
        }

        public IActionResult Levels()
        {
            List<GameLevel> levels = _locationRepository.GetAll();
            return View(levels);
        }

        public IActionResult Index(int id)
        {
            _userRepository.ResetScore();

            var level = _locationRepository.TryGetById(id);
            return View("Game", level);
        }

        [HttpPost]
        public IActionResult ProcessClick([FromBody] bool isRight)
        {
            User currentUser = _userRepository.GetCurrentUser();
            int currentScore = currentUser.Score;

            if (isRight)
            {
                _userRepository.AddScore(10);
                currentScore += 10;
            }
            else
            {
                _userRepository.SubtractScore(5);
                currentScore -= 5;
            }
            return Ok(new { newScore = currentScore });
        }

        [HttpGet]
        public IActionResult GetCurrentScore()
        {
            User currentUser = _userRepository.GetCurrentUser();
            return Ok(new { score = currentUser.Score });
        }
        // Игровая сессия портала: iframe передаёт ?session=<id>&token=<hmac>
        private int? GameSessionId
        {
            get
            {
                var raw = Request.Query["session"].ToString();
                return int.TryParse(raw, out var id) ? id : (int?)null;
            }
        }

        private string? GameSessionToken => Request.Query["token"].ToString();

        public async Task<IActionResult> AddPointsUser()
        {
            User currentUser = _userRepository.GetCurrentUser();
            int userScore = currentUser.Score;

            try
            {
                var result = await api!.AddScoreAsync(GameSessionId ?? 0, GameSessionToken, userScore);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}