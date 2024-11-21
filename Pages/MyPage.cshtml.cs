using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleApp.Data;
using SimpleApp.Models;

namespace SimpleApp.Pages
{
    public class MyPageModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        
        public MyPageModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public string FirstName { get; set; }
        public int Points{ get; set; }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                FirstName = user.FirstName; // Eller använd FirstName om det finns
                Points = user.Points ?? 0; // Hämtar poäng, eller 0 om inga poäng är angivna
            }
        }
        
    }
}
