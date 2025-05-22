using Microsoft.AspNetCore.Identity;
using SimpleApp.Models;

namespace SimpleApp.Middleware
{
    public class TeamSelectionMiddleware
    {
        private readonly RequestDelegate _next;

        public TeamSelectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var path = context.Request.Path.Value?.ToLower();
                
                // Skip middleware for these paths
                if (path != "/selectteam" && 
                    !path.StartsWith("/identity/") && 
                    !path.Contains("signout") && 
                    !path.Contains(".css") && 
                    !path.Contains(".js") && 
                    !path.Contains(".png") && 
                    !path.Contains(".ico"))
                {
                    var requireTeamSelection = context.Request.Cookies["RequireTeamSelection"];
                    if (requireTeamSelection == "true")
                    {
                        context.Response.Redirect("/SelectTeam");
                        return;
                    }

                    var user = await userManager.GetUserAsync(context.User);
                    if (user != null && string.IsNullOrEmpty(user.FavoriteTeam))
                    {
                        context.Response.Redirect("/SelectTeam");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class TeamSelectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTeamSelection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TeamSelectionMiddleware>();
        }
    }
}
