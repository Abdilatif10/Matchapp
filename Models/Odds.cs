namespace SimpleApp.Models
{
    public class Odds
    {
        public double HomeWin { get; set; } // Oddsen för att hemmalaget vinner
        public double Draw { get; set; }    // Oddsen för oavgjort
        public double AwayWin { get; set; } // Oddsen för att bortalaget vinner
    }
}
