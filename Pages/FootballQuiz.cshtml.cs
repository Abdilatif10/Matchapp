using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Models;
using SimpleApp.Services;

namespace SimpleApp.Pages
{
    [Authorize]
    public class FootballQuizModel : PageModel
    {
        private readonly FootballQuizService _quizService;
        private readonly UserManager<User> _userManager;

        [BindProperty]
        public List<int> UserAnswers { get; set; } = new();
        
        public List<QuizQuestion> Questions { get; private set; } = new();
        
        public bool QuizCompleted { get; set; }
        public int Score { get; set; }
        public string? Message { get; set; }
        public Dictionary<int, bool> AnswerResults { get; set; } = new();
        public User? CurrentUser { get; set; }

        public FootballQuizModel(FootballQuizService quizService, UserManager<User> userManager)
        {
            _quizService = quizService;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            Questions = await _quizService.GetRandomQuestionsAsync();
            UserAnswers = new List<int>(new int[Questions.Count]);
            QuizCompleted = false;
            CurrentUser = await _userManager.GetUserAsync(User);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Questions = await _quizService.GetRandomQuestionsAsync();
            CurrentUser = await _userManager.GetUserAsync(User);
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var correctAnswers = 0;
            var totalPoints = 0;

            for (int i = 0; i < Questions.Count; i++)
            {
                bool isCorrect = UserAnswers[i] == Questions[i].CorrectAnswerIndex;
                AnswerResults[i] = isCorrect;
                
                if (isCorrect)
                {
                    correctAnswers++;
                    totalPoints += Questions[i].Points;
                }
            }

            Score = totalPoints;
            QuizCompleted = true;

            if (CurrentUser != null)
            {
                CurrentUser.Points += totalPoints;
                await _userManager.UpdateAsync(CurrentUser);
            }

            Message = $"You got {correctAnswers} out of {Questions.Count} questions correct! You earned {totalPoints} points!";

            return Page();
        }
    }
}
