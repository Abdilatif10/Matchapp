using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BettingApp.Data;
using BettingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BettingApp.Pages
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
           
            Users = await _dbContext.Users
                .OrderByDescending(u => u.Points)  
                .ToListAsync();
        }
    }
}
