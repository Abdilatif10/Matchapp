using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SimpleApp.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Förnamn är obligatoriskt.")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Förnamnet måste vara mellan 1 och 30 tecken.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Födelsedatum är obligatoriskt.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public double? Points { get; set; } = 100;

        [StringLength(30, MinimumLength = 1, ErrorMessage = "Efternamnet måste vara mellan 1 och 30 tecken.")]
        public string? LastName { get; set; }

        public string? ProfilePicture { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? FavoriteTeam { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}