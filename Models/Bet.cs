namespace BettingApp.Models
{
    public class Bet
    {
        public int Id { get; set; }
        public string UserId { get; set; } // F�r att koppla till anv�ndaren
        public int MatchId { get; set; } // Koppling till vald match
        public double Amount { get; set; } // Insats
        public string BetType { get; set; } // "HomeWin", "Draw", "AwayWin"
        public bool IsSettled { get; set; } // Indikator f�r om spelet �r avgjort
        public double PotentialPayout { get; set; } // M�jlig vinst
        public double HomeWinOdds { get; set; }
        public double DrawOdds { get; set; }
        public double AwayWinOdds { get; set; }
        public bool IsPayoutDone { get; set; }







    }
}
