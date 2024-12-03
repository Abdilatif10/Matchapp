namespace SimpleApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Crest { get; set; }
        public int TeamRatingScale { get; set; }
    }


    //Lägg till alla 20 PL lag i en lista och ge de ett värde på 1-10 på TeamRatingScale--- 1 är sämst och 10 är bäst
    //Logiken för Oddsen måste kommunicera med TeamRatingScale för att ge rätt odds
    //Vi måste kunna använda TeamRatingScale värdet för att kunna ge rätt odds och veta vilket lag som är favorit
    // if(homeTeam.TeamRatingScale > awayTeam.TeamRatingScale)
    // {home.TeamRatingScale = favouriteTeam
    // = ge odds för favoruiteteam }
}

