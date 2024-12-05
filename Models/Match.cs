
namespace SimpleApp.Models
{
    public class Match
    {
        public Competition Competition { get; set; }
        public int Id { get; set; }
        public DateTime UtcDate { get; set; }
        public string Status { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Score Score { get; set; }
        public Odds Odds { get; set; }
        public User User { get; set; }


    }
}