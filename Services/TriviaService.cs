using System.Text.Json;
using SimpleApp.Models;

namespace SimpleApp.Services
{
    public class TriviaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TriviaService> _logger;

        public TriviaService(HttpClient httpClient, ILogger<TriviaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<QuizQuestion>> GetTriviaQuestionsAsync(int count = 5)
        {
            try
            {
                string url = $"https://the-trivia-api.com/v2/questions?tags=football&limit={count}";
                var response = await _httpClient.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var triviaQuestions = JsonSerializer.Deserialize<List<TriviaQuestion>>(response, options);

                // Convert TriviaQuestion to our QuizQuestion format
                var quizQuestions = new List<QuizQuestion>();
                int id = 100; // Start from 100 to avoid conflicts with static questions

                foreach (var q in triviaQuestions)
                {
                    var allAnswers = new List<string>(q.IncorrectAnswers) { q.CorrectAnswer };
                    Shuffle(allAnswers);
                    
                    var correctIndex = allAnswers.IndexOf(q.CorrectAnswer);
                    
                    quizQuestions.Add(new QuizQuestion
                    {
                        Id = id++,
                        Question = q.Question.Text,
                        Options = allAnswers,
                        CorrectAnswerIndex = correctIndex,
                        Points = 20, // Higher points for trivia questions
                        Explanation = $"The correct answer is: {q.CorrectAnswer}"
                    });
                }

                return quizQuestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trivia questions");
                return new List<QuizQuestion>();
            }
        }

        private static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
    }    // API response models
    public class TriviaQuestion
    {
        public string Id { get; set; }
        public QuestionText Question { get; set; }
        public string CorrectAnswer { get; set; }
        public List<string> IncorrectAnswers { get; set; }
    }

    public class QuestionText
    {
        public string Text { get; set; }
    }
}
