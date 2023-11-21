using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PhotosApp.Areas.Identity.Data;
using PhotosApp.Services;
using PhotosApp.Services.Authorization;
using PhotosApp.Services.TicketStores;

[assembly: HostingStartup(typeof(PhotosApp.Areas.Identity.IdentityHostingStartup))]
namespace PhotosApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            // builder.ConfigureServices((context, services) =>
            // {
            //     services.AddDbContext<UsersDbContext>(options =>
            //         options.UseSqlite(
            //             context.Configuration.GetConnectionString("UsersDbContextConnection")));
            //
            //     services.AddDbContext<TicketsDbContext>(options =>
            //         options.UseSqlite(
            //             context.Configuration.GetConnectionString("TicketsDbContextConnection")));
            //
            //     services.AddDefaultIdentity<PhotosAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
            //         .AddRoles<IdentityRole>()
            //         .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
            //         .AddEntityFrameworkStores<UsersDbContext>()
            //         .AddPasswordValidator<UsernameAsPasswordValidator<PhotosAppUser>>()
            //         .AddErrorDescriber<RussianIdentityErrorDescriber>();
            //     
            //     services.ConfigureExternalCookie(options =>
            //     {
            //         options.Cookie.Name = "PhotosApp.Auth.External";
            //         options.Cookie.HttpOnly = true;
            //         options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //         options.SlidingExpiration = true;
            //     });
            //     
            //     services.Configure<IdentityOptions>(options =>
            //     {
            //         // Default Password settings.
            //         options.Password.RequireDigit = false;
            //         options.Password.RequireLowercase = true;
            //         options.Password.RequireNonAlphanumeric = false;
            //         options.Password.RequireUppercase = false;
            //         options.Password.RequiredLength = 6;
            //         options.Password.RequiredUniqueChars = 1;
            //     });
            //     
            //     services.Configure<IdentityOptions>(options =>
            //     {
            //         // Default SignIn settings.
            //         options.SignIn.RequireConfirmedEmail = false;
            //         options.SignIn.RequireConfirmedPhoneNumber = false;
            //         options.SignIn.RequireConfirmedAccount = false;
            //     });
            //     
            //     services.AddTransient<EntityTicketStore>();
            //     services.ConfigureApplicationCookie(options =>
            //     {
            //         var serviceProvider = services.BuildServiceProvider();
            //         // options.SessionStore = serviceProvider.GetRequiredService<EntityTicketStore>();
            //         options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //         options.Cookie.Name = "PhotosApp.Auth";
            //         options.Cookie.HttpOnly = true;
            //         options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            //         options.LoginPath = "/Identity/Account/Login";
            //         // ReturnUrlParameter requires 
            //         //using Microsoft.AspNetCore.Authentication.Cookies;
            //         options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            //         options.SlidingExpiration = true;
            //     });
            //     
            //     services.AddScoped<IPasswordHasher<PhotosAppUser>, SimplePasswordHasher<PhotosAppUser>>();
            //         
            //     services.AddAuthentication()
            //         // .AddGoogle("Google", options =>
            //         // {
            //         //     options.ClientId = context.Configuration["Authentication:Google:ClientId"];
            //         //     options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
            //         // });
            //         .AddOpenIdConnect(
            //         authenticationScheme: "Google",
            //         displayName: "Google",
            //         options =>
            //         {
            //             options.Authority = "https://accounts.google.com/";
            //             options.ClientId = context.Configuration["Authentication:Google:ClientId"];
            //             options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
            //
            //             options.CallbackPath = "/signin-google";
            //             options.SignedOutCallbackPath = "/signout-callback-google";
            //             options.RemoteSignOutPath = "/signout-google";
            //
            //             options.Scope.Add("email");
            //         });
            //     services.AddAuthentication()
            //         .AddJwtBearer(options =>
            //         {
            //             options.RequireHttpsMetadata = false;
            //             options.TokenValidationParameters = new TokenValidationParameters
            //             {
            //                 ValidateIssuer = false,
            //                 ValidateAudience = false,
            //                 ValidateLifetime = true,
            //                 ClockSkew = TimeSpan.Zero,
            //                 ValidateIssuerSigningKey = true,
            //                 IssuerSigningKey = TemporaryTokens.SigningKey
            //             };
            //             options.Events = new JwtBearerEvents
            //             {
            //                 OnMessageReceived = c =>
            //                 {
            //                     c.Token = c.Request.Cookies["TemporaryToken"];
            //                     return Task.CompletedTask;
            //                 }
            //             };
            //         });
            // });
        }
    }
}