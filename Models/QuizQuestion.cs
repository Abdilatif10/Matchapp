namespace SimpleApp.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; }
        public string? TeamName { get; set; }
        public int Points { get; set; } = 10;
        public string? Explanation { get; set; }
    }
}
