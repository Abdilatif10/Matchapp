using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Data;
using SimpleApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleApp.Pages
{
    public class LeaderBoardModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;

        public List<User> Users { get; set; }

        public LeaderBoardModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            // Hämta alla användare med deras poäng från databasen
            Users = await _dbContext.Users
                .OrderByDescending(u => u.Points)  // Sortera användarna efter poäng
                .ToListAsync();
        }
    }
}
