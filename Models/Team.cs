namespace BettingApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Crest { get; set; }
        public int TeamRatingScale { get; set; }
    }


    //L�gg till alla 20 PL lag i en lista och ge de ett v�rde p� 1-10 p� TeamRatingScale--- 1 �r s�mst och 10 �r b�st
    //Logiken f�r Oddsen m�ste kommunicera med TeamRatingScale f�r att ge r�tt odds
    //Vi m�ste kunna anv�nda TeamRatingScale v�rdet f�r att kunna ge r�tt odds och veta vilket lag som �r favorit
    // if(homeTeam.TeamRatingScale > awayTeam.TeamRatingScale)
    // {home.TeamRatingScale = favouriteTeam
    // = ge odds f�r favoruiteteam }

    //Den h�r kommentaren �r f�r att testa att jag kan kommentera och se namnet p� commiten ist�llet f�r hashen.



}

