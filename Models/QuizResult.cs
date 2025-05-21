namespace SimpleApp.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
