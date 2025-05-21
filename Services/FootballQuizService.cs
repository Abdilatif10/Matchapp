using SimpleApp.Models;
using SimpleApp.Services.Enums;

namespace SimpleApp.Services
{    public class FootballQuizService
    {
        private readonly List<QuizQuestion> _staticQuestions;
        private readonly FootballDataService _footballDataService;
        private readonly TriviaService _triviaService;

        public FootballQuizService(FootballDataService footballDataService, TriviaService triviaService)
        {
            _footballDataService = footballDataService;
            _triviaService = triviaService;
            _staticQuestions = GenerateStaticQuestions();
        }

        public async Task<List<QuizQuestion>> GetRandomQuestionsAsync(int count = 10)
        {
            var rng = new Random();            var questions = new List<QuizQuestion>();
            
            // Get trivia questions
            var triviaQuestions = await _triviaService.GetTriviaQuestionsAsync(3);
            
            // Add dynamic questions based on current standings
            var standings = await _footballDataService.GetPremierLeagueStandingsAsync();
            var dynamicQuestions = GenerateDynamicQuestions(standings);
            
            // Combine static, dynamic, and trivia questions
            var allQuestions = _staticQuestions
                .Concat(dynamicQuestions)
                .Concat(triviaQuestions)
                .ToList();
            
            return allQuestions.OrderBy(x => rng.Next()).Take(count).ToList();
        }

        private List<QuizQuestion> GenerateDynamicQuestions(List<Standing> standings)
        {
            var questions = new List<QuizQuestion>();
            
            if (standings.Any())
            {
                // Add question about current league leader
                var leader = standings.First();
                questions.Add(new QuizQuestion
                {
                    Id = 11,
                    Question = "Which team is currently leading the Premier League?",
                    Options = standings.Take(4).Select(s => s.Team?.Name ?? "Unknown").ToList(),
                    CorrectAnswerIndex = 0,
                    TeamName = leader.Team?.Name,
                    Points = 15,
                    Explanation = $"{leader.Team?.Name} is currently leading with {leader.Points} points after {leader.PlayedGames} games."
                });

                // Add question about goals
                var topScorer = standings.OrderByDescending(s => s.GoalsFor).First();
                var scorerOptions = standings.OrderByDescending(s => s.GoalsFor).Take(4)
                    .Select(s => s.Team?.Name ?? "Unknown").ToList();
                questions.Add(new QuizQuestion
                {
                    Id = 12,
                    Question = "Which team has scored the most goals this season?",
                    Options = scorerOptions,
                    CorrectAnswerIndex = 0,
                    TeamName = topScorer.Team?.Name,
                    Points = 15,
                    Explanation = $"{topScorer.Team?.Name} has scored {topScorer.GoalsFor} goals this season."
                });

                // Add question about best defense
                var bestDefense = standings.OrderBy(s => s.GoalsAgainst).First();
                var defenseOptions = standings.OrderBy(s => s.GoalsAgainst).Take(4)
                    .Select(s => s.Team?.Name ?? "Unknown").ToList();
                questions.Add(new QuizQuestion
                {
                    Id = 13,
                    Question = "Which team has conceded the fewest goals this season?",
                    Options = defenseOptions,
                    CorrectAnswerIndex = 0,
                    TeamName = bestDefense.Team?.Name,
                    Points = 15,
                    Explanation = $"{bestDefense.Team?.Name} has only conceded {bestDefense.GoalsAgainst} goals this season."
                });

                // Add question about wins
                var mostWins = standings.OrderByDescending(s => s.Won).First();
                var winsOptions = standings.OrderByDescending(s => s.Won).Take(4)
                    .Select(s => s.Team?.Name ?? "Unknown").ToList();
                questions.Add(new QuizQuestion
                {
                    Id = 14,
                    Question = "Which team has the most wins this season?",
                    Options = winsOptions,
                    CorrectAnswerIndex = 0,
                    TeamName = mostWins.Team?.Name,
                    Points = 15,
                    Explanation = $"{mostWins.Team?.Name} has won {mostWins.Won} games this season."
                });
            }

            return questions;
        }

        private List<QuizQuestion> GenerateStaticQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Id = 1,
                    Question = "Which team has won the most Premier League titles?",
                    Options = new List<string>
                    {
                        "Manchester United",
                        "Liverpool",
                        "Arsenal",
                        "Chelsea"
                    },
                    CorrectAnswerIndex = 0,
                    TeamName = "Manchester United",
                    Explanation = "Manchester United has won 13 Premier League titles."
                },
                new QuizQuestion
                {
                    Id = 2,
                    Question = "Which Premier League team is known as 'The Citizens'?",
                    Options = new List<string>
                    {
                        "Manchester United",
                        "Manchester City",
                        "Newcastle United",
                        "Leeds United"
                    },
                    CorrectAnswerIndex = 1,
                    TeamName = "Manchester City",
                    Explanation = "Manchester City's nickname is 'The Citizens'."
                },
                new QuizQuestion
                {
                    Id = 3,
                    Question = "What is Liverpool FC's home stadium?",
                    Options = new List<string>
                    {
                        "Old Trafford",
                        "Emirates Stadium",
                        "Anfield",
                        "Stamford Bridge"
                    },
                    CorrectAnswerIndex = 2,
                    TeamName = "Liverpool FC",
                    Explanation = "Anfield has been Liverpool's home stadium since 1892."
                },
                new QuizQuestion
                {
                    Id = 4,
                    Question = "Which team has the nickname 'The Gunners'?",
                    Options = new List<string>
                    {
                        "Arsenal",
                        "Tottenham",
                        "Chelsea",
                        "West Ham"
                    },
                    CorrectAnswerIndex = 0,
                    TeamName = "Arsenal",
                    Explanation = "Arsenal's nickname is 'The Gunners' due to the club's formation by workers at the Royal Arsenal."
                },
                new QuizQuestion
                {
                    Id = 5,
                    Question = "What is the capacity of Tottenham Hotspur Stadium?",
                    Options = new List<string>
                    {
                        "50,000",
                        "62,850",
                        "75,000",
                        "40,000"
                    },
                    CorrectAnswerIndex = 1,
                    TeamName = "Tottenham Hotspur",
                    Explanation = "The Tottenham Hotspur Stadium has a capacity of 62,850, making it one of the largest in the Premier League."
                },
                new QuizQuestion
                {
                    Id = 6,
                    Question = "Which team's home kit is traditionally royal blue?",
                    Options = new List<string>
                    {
                        "Manchester City",
                        "Arsenal",
                        "Chelsea",
                        "Liverpool"
                    },
                    CorrectAnswerIndex = 2,
                    TeamName = "Chelsea",
                    Explanation = "Chelsea has traditionally worn royal blue shirts since the club's formation."
                },
                new QuizQuestion
                {
                    Id = 7,
                    Question = "Which of these teams is rated as TopTier in our system?",
                    Options = new List<string>
                    {
                        "Manchester City",
                        "Aston Villa",
                        "Brighton",
                        "Brentford"
                    },
                    CorrectAnswerIndex = 0,
                    TeamName = "Manchester City",
                    Explanation = $"Manchester City is rated as {TeamRating.TopTier} in our rating system."
                },
                new QuizQuestion
                {
                    Id = 8,
                    Question = "Which team plays their home games at St. James' Park?",
                    Options = new List<string>
                    {
                        "Aston Villa",
                        "Newcastle United",
                        "Brighton",
                        "Crystal Palace"
                    },
                    CorrectAnswerIndex = 1,
                    TeamName = "Newcastle United",
                    Explanation = "Newcastle United has played at St. James' Park since 1892."
                },
                new QuizQuestion
                {
                    Id = 9,
                    Question = "Which team has a cannon in their crest?",
                    Options = new List<string>
                    {
                        "Arsenal",
                        "West Ham",
                        "Manchester United",
                        "Liverpool"
                    },
                    CorrectAnswerIndex = 0,
                    TeamName = "Arsenal",
                    Explanation = "Arsenal's crest features a cannon, reflecting the club's origins at the Royal Arsenal in Woolwich."
                },
                new QuizQuestion
                {
                    Id = 10,
                    Question = "Which team's motto is 'Nil Satis Nisi Optimum' (Nothing but the best is good enough)?",
                    Options = new List<string>
                    {
                        "Liverpool",
                        "Manchester United",
                        "Everton",
                        "Tottenham"
                    },
                    CorrectAnswerIndex = 2,
                    TeamName = "Everton",
                    Explanation = "Everton's Latin motto 'Nil Satis Nisi Optimum' has been part of the club's tradition since 1938."
                }
            };
        }
    }
}
