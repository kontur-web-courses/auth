using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            builder.ConfigureServices((context, services) => {
                services.AddTransient<IEmailSender, SimpleEmailSender>(serviceProvider =>
                    new SimpleEmailSender(
                        serviceProvider.GetRequiredService<ILogger<SimpleEmailSender>>(),
                        serviceProvider.GetRequiredService<IWebHostEnvironment>(),
                        context.Configuration["SimpleEmailSender:Host"],
                        context.Configuration.GetValue<int>("SimpleEmailSender:Port"),
                        context.Configuration.GetValue<bool>("SimpleEmailSender:EnableSSL"),
                        context.Configuration["SimpleEmailSender:UserName"],
                        context.Configuration["SimpleEmailSender:Password"]
                    ));
                
                services.AddDefaultIdentity<PhotosAppUser>()
                    .AddRoles<IdentityRole>()
                    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
                    .AddEntityFrameworkStores<UsersDbContext>()
                    .AddEntityFrameworkStores<TicketsDbContext>()
                    .AddPasswordValidator<UsernameAsPasswordValidator<PhotosAppUser>>()
                    .AddErrorDescriber<RussianIdentityErrorDescriber>();
                
                services.AddDbContext<UsersDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("UsersDbContextConnection")));
                
                services.AddDbContext<TicketsDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("TicketsDbContextConnection")));
                
                services.Configure<IdentityOptions>(options =>
                {
                    // Default Password settings.
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;
                });
                
                services.Configure<IdentityOptions>(options =>
                {
                    // Default SignIn settings.
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    options.SignIn.RequireConfirmedAccount = false;
                });
                
                services.AddTransient<EntityTicketStore>();
                services.ConfigureApplicationCookie(options =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    //options.SessionStore = serviceProvider.GetRequiredService<EntityTicketStore>();
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.Cookie.Name = "PhotosApp.Auth";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Identity/Account/Login";
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                });
                
                services.ConfigureExternalCookie(options =>
                {
                    options.Cookie.Name = "PhotosApp.Auth.External";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.SlidingExpiration = true;
                });
                
                services.AddScoped<IPasswordHasher<PhotosAppUser>, SimplePasswordHasher<PhotosAppUser>>();
                
                services.AddAuthentication()
                    /*.AddGoogle("Google", options =>
                    {
                        options.ClientId = context.Configuration["Authentication:Google:ClientId"];
                        options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
                    });*/
                    .AddOpenIdConnect(
                    authenticationScheme: "Google",
                    displayName: "Google",
                    options =>
                    {
                        options.Authority = "https://accounts.google.com/";
                        options.ClientId = context.Configuration["Authentication:Google:ClientId"];
                        options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];

                        options.CallbackPath = "/signin-google";
                        options.SignedOutCallbackPath = "/signout-callback-google";
                        options.RemoteSignOutPath = "/signout-google";

                        options.Scope.Add("email");
                    });
                
                services.AddAuthentication()
                    .AddOpenIdConnect("Passport", "Паспорт", options =>
                    {
                        options.Authority = "https://localhost:7001";

                        options.ClientId = "Photos App by OIDC";
                        options.ClientSecret = "secret";
                        options.ResponseType = "code";

                        // NOTE: oidc и profile уже добавлены по-умолчанию
                        options.Scope.Add("email");

                        options.CallbackPath = "/signin-passport";

                        // NOTE: все эти проверки токена выполняются по умолчанию, указаны для ознакомления
                        options.TokenValidationParameters.ValidateIssuer = true; // проверка издателя
                        options.TokenValidationParameters.ValidateAudience = true; // проверка получателя
                        options.TokenValidationParameters.ValidateLifetime = true; // проверка не протух ли
                        options.TokenValidationParameters.RequireSignedTokens = true; // есть ли валидная подпись издателя
                    });
                
                services.AddAuthentication()
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = TemporaryTokens.SigningKey
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = c =>
                            {
                                c.Token = c.Request.Cookies[TemporaryTokens.CookieName];
                                return Task.CompletedTask;
                            }
                        };
                    });
                
                services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>();
                
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(
                            JwtBearerDefaults.AuthenticationScheme,
                            IdentityConstants.ApplicationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                    options.AddPolicy(
                        "Beta",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("testing", "beta");
                        });
                    options.AddPolicy(
                        "CanAddPhoto",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.RequireClaim("subscription", "paid");
                        });
                    options.AddPolicy(
                        "MustOwnPhoto",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            policyBuilder.AddRequirements(new MustOwnPhotoRequirement());
                        });
                    options.AddPolicy(
                        "Dev",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();
                            /*policyBuilder.RequireRole("Dev");
                            policyBuilder.AddAuthenticationSchemes(
                                JwtBearerDefaults.AuthenticationScheme,
                                IdentityConstants.ApplicationScheme);*/
                        });
                });
            });
        }
    }
}