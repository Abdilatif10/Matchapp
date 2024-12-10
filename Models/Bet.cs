namespace SimpleApp.Models
{
    public class Bet
    {
        public int Id { get; set; }
        public string UserId { get; set; } // För att koppla till användaren
        public int MatchId { get; set; } // Koppling till vald match
        public double Amount { get; set; } // Insats
        public string BetType { get; set; } // "HomeWin", "Draw", "AwayWin"
        public bool IsSettled { get; set; } // Indikator för om spelet är avgjort
        public double PotentialPayout { get; set; } // Möjlig vinst
        public double HomeWinOdds { get; set; }
        public double DrawOdds { get; set; }
        public double AwayWinOdds { get; set; }
        public bool IsPayoutDone { get; set; }







    }
}
