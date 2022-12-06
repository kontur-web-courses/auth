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
                services.AddDbContext<UsersDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("UsersDbContextConnection")));
                
                services.AddDbContext<TicketsDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("TicketsDbContextConnection")));
                
                services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>();

                services.AddDefaultIdentity<PhotosAppUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddRoles<IdentityRole>()
                    .AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>()
                    .AddPasswordValidator<UsernameAsPasswordValidator<PhotosAppUser>>()
                    .AddErrorDescriber<RussianIdentityErrorDescriber>()
                    .AddEntityFrameworkStores<UsersDbContext>();
                services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityConstants.ApplicationScheme;
                    o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                });
                services.AddAuthentication()
                    .AddOpenIdConnect("Passport", "ÐÐ°ÑÐ¿Ð¾ÑÑ", options =>
                    {
                        options.Authority = "TODO: Ð°Ð´ÑÐµÑ ÑÐµÑÐ²ÐµÑÐ° Ð°Ð²ÑÐ¾ÑÐ¸Ð·Ð°ÑÐ¸Ð¸";

                        options.ClientId = "Photos App by OIDC";
                        options.ClientSecret = "secret";
                        options.ResponseType = "code";

                        // NOTE: oidc Ð¸ profile ÑÐ¶Ðµ Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½Ñ Ð¿Ð¾ ÑÐ¼Ð¾Ð»ÑÐ°Ð½Ð¸Ñ
                        options.Scope.Add();

                        options.CallbackPath = "https://localhost:5001/signin-passport";

                        // NOTE: Ð²ÑÐµ ÑÑÐ¸ Ð¿ÑÐ¾Ð²ÐµÑÐºÐ¸ ÑÐ¾ÐºÐµÐ½Ð° Ð²ÑÐ¿Ð¾Ð»Ð½ÑÑÑÑÑ Ð¿Ð¾ ÑÐ¼Ð¾Ð»ÑÐ°Ð½Ð¸Ñ, ÑÐºÐ°Ð·Ð°Ð½Ñ Ð´Ð»Ñ Ð¾Ð·Ð½Ð°ÐºÐ¾Ð¼Ð»ÐµÐ½Ð¸Ñ
                        options.TokenValidationParameters.ValidateIssuer = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð¸Ð·Ð´Ð°ÑÐµÐ»Ñ
                        options.TokenValidationParameters.ValidateAudience = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð¿Ð¾Ð»ÑÑÐ°ÑÐµÐ»Ñ
                        options.TokenValidationParameters.ValidateLifetime = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð½Ðµ Ð¿ÑÐ¾ÑÑÑ
                        options.TokenValidationParameters.RequireSignedTokens = true; // ÐµÑÑÑ Ð»Ð¸ Ð²Ð°Ð»Ð¸Ð´Ð½Ð°Ñ Ð¿Ð¾Ð´Ð¿Ð¸ÑÑ Ð¸Ð·Ð´Ð°ÑÐµÐ»Ñ
                    });

                services.ConfigureExternalCookie(options =>
                {
                    options.Cookie.Name = "PhotosApp.Auth.External";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.SlidingExpiration = true;
                });
                services.Configure<IdentityOptions>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;
                });
                services.Configure<IdentityOptions>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
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
                    // ReturnUrlParameter requires 
                    //using Microsoft.AspNetCore.Authentication.Cookies;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                });
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(
                            JwtBearerDefaults.AuthenticationScheme,
                            IdentityConstants.ApplicationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                    options.AddPolicy(
                        "Dev",
                        policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser();/*
                            policyBuilder.RequireRole("Dev");
                            policyBuilder.AddAuthenticationSchemes(
                                JwtBearerDefaults.AuthenticationScheme, 
                                IdentityConstants.ApplicationScheme);*/
                        });
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
                });
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
                        //options.SaveTokens = true;
                        options.Scope.Add("email");
                    });
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
                                c.Token = c.Request.Cookies["TemporaryToken"];
                                return Task.CompletedTask;
                            }
                        };

                    });


            });
        }
    }
}