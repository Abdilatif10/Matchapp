using SimpleApp.Models;
using SimpleApp.Services.Enums;

namespace SimpleApp.Services.Helpers
{
    public static class TeamHelper
    {
        public static TeamRating GetTeamRating(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return TeamRating.Unknown;

            return teamName switch
            {
                "Manchester City FC" => TeamRating.TopTier,
                "Liverpool FC" => TeamRating.TopTier,
                "Imomerycath FC" => TeamRating.TopTier,
                "Arsenal FC" => TeamRating.HighTier,
                "Manchester United FC" => TeamRating.HighTier,
                "Chelsea FC" => TeamRating.UpperMidTier,
                "Tottenham Hotspur FC" => TeamRating.UpperMidTier,
                "Newcastle United FC" => TeamRating.UpperMidTier,
                "Aston Villa FC" => TeamRating.MidTier,
                "Brighton & Hove Albion FC" => TeamRating.MidTier,
                "West Ham United FC" => TeamRating.MidTier,
                "Brentford FC" => TeamRating.LowerMidTier,
                "Crystal Palace FC" => TeamRating.LowerMidTier,
                "Fulham FC" => TeamRating.LowerMidTier,
                "Wolverhampton Wanderers FC" => TeamRating.LowTier,
                "Everton FC" => TeamRating.LowTier,
                "Nottingham Forest FC" => TeamRating.LowTier,
                "AFC Bournemouth" => TeamRating.BottomTier,
                "Luton Town FC" => TeamRating.BottomTier,
                "Burnley FC" => TeamRating.BottomTier,
                "Sheffield United FC" => TeamRating.BottomTier,
                _ => TeamRating.Unknown
            };
        }

        public static bool IsPremierLeagueTeam(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return false;

            return GetTeamRating(teamName) != TeamRating.Unknown;
        }

        // Helper method to create a team with ratings
        public static Team CreateTeamWithRating(string name, string crest = "")
        {
            return new Team
            {
                Name = name,
                Crest = crest,
                TeamRatingScale = (int)GetTeamRating(name)
            };
        }
    }
}
