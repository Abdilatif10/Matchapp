
namespace SimpleApp.Models
{
    public class FootballApiResponse
    {
        public Filters Filters { get; set; }
        public ResultSet ResultSet { get; set; }
        public List<Match> Matches { get; set; }
    }

    public class Filters
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }

    public class ResultSet
    {
        public int Count { get; set; }
        public string Competitions { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public int Played { get; set; }
    }

    public class Match
    {
        public Competition Competition { get; set; }
        public Season Season { get; set; }
        public int Id { get; set; }
        public DateTime UtcDate { get; set; }
        public string Status { get; set; }
        public int Matchday { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public Score Score { get; set; }
        
    }

    public class Competition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Emblem { get; set; }
    }

    public class Season
    {
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int CurrentMatchday { get; set; }
        public string Winner { get; set; }
    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } // Använd Rename ändra omm alla variabler til stor bokstav. 
        public string Crest { get; set; }

        //Internal measurment strength skala 1-10 
        public int HowGoodistheTeam { get; set; }  
    }

    public class Score
    {
        public string Winner { get; set; }
        public string Duration { get; set; }
        public FullTime FullTime { get; set; }
    }

    public class FullTime
    {
        public int? Home { get; set; }
        public int? Away { get; set; }
    }

}