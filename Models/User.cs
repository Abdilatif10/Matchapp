using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BettingApp.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "F�rnamn �r obligatoriskt.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "F�rnamnet m�ste vara mellan 1 och 30 tecken.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "F�delsedatum �r obligatoriskt.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public double? Points { get; set; } = 100; //Standardv�rde

       


    }
}
