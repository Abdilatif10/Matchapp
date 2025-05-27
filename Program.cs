using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleApp.Data;
using SimpleApp.Models;
using SimpleApp.Services;
using SimpleApp.Services.Interfaces;

namespace SimpleApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            // Configure database and identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            
            builder.Services.AddScoped<SignInManager<User>, CustomSignInManager>();            
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient<FootballDataService>();

            
            builder.Services.AddScoped<IFootballDataService, FootballDataService>();
            builder.Services.AddScoped<FootballDataService>();
            builder.Services.AddScoped<FootballQuizService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

       

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}

