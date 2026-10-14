using DotNetEnv;
using System.Text;
using System.Text.Json;

namespace SearchingForItems.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            // адрес API портала Рудзынг.рф (переопределяется переменной окружения)
            string baseUrl = Env.GetString("LINGVO_API_URL", "http://game.рудзынг.рф");

            this._httpClient = new HttpClient();
            this._httpClient.BaseAddress = new System.Uri(baseUrl);
            this._httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<GameSessionResponse?> ResolveGameSessionAsync(int gameSessionId, string? token)
        {
            if (gameSessionId <= 0)
                return null;

            var request = new { gameSessionId, token };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/game/session/resolve", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Игровая сессия отклонена: {responseContent}");

            var result = JsonSerializer.Deserialize<GameSessionResponse>(responseContent);
            if (result is null || !result.valid || string.IsNullOrWhiteSpace(result.userId))
                throw new Exception("Портал вернул некорректную игровую сессию.");
            var expectedGameId = Environment.GetEnvironmentVariable("LINGVO_GAME_ID");
            if (!int.TryParse(expectedGameId, out var gameId) || gameId <= 0)
                throw new Exception("LINGVO_GAME_ID не настроен для этой игры.");
            if (result.gameId != gameId)
                throw new Exception("Игровая сессия выдана для другой игры.");
            return result;
        }

        /// <summary>
        /// Начисление баллов через игровую сессию портала.
        /// gameSessionId и token приходят в query (?session=...&token=...),
        /// когда игра запущена в iframe портала Рудзынг.рф.
        /// </summary>
        public async Task<ScoreResponse?> AddScoreAsync(int gameSessionId, string? token, int amount)
        {
            if (gameSessionId <= 0)
                throw new Exception("Игра запущена не с портала: сессия не выдана.");

            var scoreData = new { gameSessionId = gameSessionId, score = amount, token = token };
            var json = JsonSerializer.Serialize(scoreData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/score", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Ошибка начисления баллов: {responseContent}");

            return JsonSerializer.Deserialize<ScoreResponse>(responseContent);
        }
    }

    public class GameSessionResponse
    {
        public bool valid { get; set; }
        public int gameId { get; set; }
        public string userId { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public int totalUserPoints { get; set; }
    }

    public class ScoreResponse
    {
        public string userId { get; set; } = string.Empty;
        public int newTotalPoints { get; set; }
    }

}
