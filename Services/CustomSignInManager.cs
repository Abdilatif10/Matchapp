using Microsoft.AspNetCore.Identity;
using SimpleApp.Models;

namespace SimpleApp.Services
{
    public class CustomSignInManager : SignInManager<User>
    {
        public CustomSignInManager(
            UserManager<User> userManager,
            Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor,
            Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<User> claimsFactory,
            Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
            Microsoft.Extensions.Logging.ILogger<SignInManager<User>> logger,
            Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes,
            Microsoft.AspNetCore.Identity.IUserConfirmation<User> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
              if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user != null && string.IsNullOrEmpty(user.FavoriteTeam))
                {
                    Context.Response.Cookies.Append("RequireTeamSelection", "true");
                }
            }
            
            return result;
        }
    }
}
