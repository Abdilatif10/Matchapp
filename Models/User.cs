using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleApp.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Förnamnet måste vara mellan 1 och 30 tecken.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Födelsedatum är obligatoriskt.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public int? Points { get; set; } = 100; //Standardvärde

       


    }
}