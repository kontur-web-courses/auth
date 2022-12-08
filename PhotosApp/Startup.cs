using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PhotosApp.Clients;
using PhotosApp.Clients.Models;
using PhotosApp.Data;
using PhotosApp.Models;
using PhotosApp.Services.Authorization;
using Serilog;

namespace PhotosApp
{
    public class Startup
    {
        private IWebHostEnvironment env { get; }
        private IConfiguration configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PhotosServiceOptions>(configuration.GetSection("PhotosService"));
            
            var mvc = services.AddControllersWithViews();
            services.AddRazorPages();
            if (env.IsDevelopment())
                mvc.AddRazorRuntimeCompilation();

            // NOTE: Подключение IHttpContextAccessor, чтобы можно было получать HttpContext там,
            // где это не получается сделать более явно.
            services.AddHttpContextAccessor();

            var connectionString = configuration.GetConnectionString("PhotosDbContextConnection")
                ?? "Data Source=PhotosApp.db";
            services.AddDbContext<PhotosDbContext>(o => o.UseSqlite(connectionString));
            // NOTE: Вместо Sqlite можно использовать LocalDB от Microsoft или другой SQL Server
            //services.AddDbContext<PhotosDbContext>(o =>
            //    o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=PhotosApp;Trusted_Connection=True;"));

            //services.AddScoped<IPhotosRepository, LocalPhotosRepository>();
            services.AddScoped<IPhotosRepository, RemotePhotosRepository>();

            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<PhotoEntity, PhotoDto>().ReverseMap();
                cfg.CreateMap<PhotoEntity, Photo>().ReverseMap();

                cfg.CreateMap<EditPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            }, new System.Reflection.Assembly[0]);

            services.AddTransient<ICookieManager, ChunkingCookieManager>();
                
                    services.AddAuthorization(options =>
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
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
                    services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>(); 
                    const string oidcAuthority = "https://localhost:7001";
                    var oidcConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        $"{oidcAuthority}/.well-known/openid-configuration",
                        new OpenIdConnectConfigurationRetriever(),
                        new HttpDocumentRetriever());
                    services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(oidcConfigurationManager);

                    services.AddAuthentication()
                    .AddOpenIdConnect("Passport", "Паспорт", options =>
                    {
                        options.ConfigurationManager = oidcConfigurationManager;
                        options.Authority = oidcAuthority;


                        options.ClientId = "Photos App by OIDC";
                        options.ClientSecret = "secret";
                        options.ResponseType = "code";
                        
                        options.Scope.Add(IdentityServerConstants.StandardScopes.Email);
                        options.Scope.Add("photos_app");
                        options.Scope.Add("photos");
                        options.Scope.Add("offline_access");


                        options.CallbackPath = "/signin-passport";
                        options.SignedOutCallbackPath = "/signout-callback-passport";
                        options.SaveTokens = true;
                        options.Events = new OpenIdConnectEvents
                        {
                            OnTokenResponseReceived = context =>
                            {
                                var tokenResponse = context.TokenEndpointResponse;
                                var tokenHandler = new JwtSecurityTokenHandler();

                                SecurityToken accessToken = null;
                                if (tokenResponse.AccessToken != null)
                                {
                                    accessToken = tokenHandler.ReadToken(tokenResponse.AccessToken);
                                }

                                SecurityToken idToken = null;
                                if (tokenResponse.IdToken != null)
                                {
                                    idToken = tokenHandler.ReadToken(tokenResponse.IdToken);
                                }

                                string refreshToken = null;
                                if (tokenResponse.RefreshToken != null)
                                {
                                    // NOTE: Это не JWT-токен
                                    refreshToken = tokenResponse.RefreshToken;
                                }

                                return Task.CompletedTask;
                            },
                            OnRemoteFailure = context => 
                            {
                                context.Response.Redirect("/");
                                context.HandleResponse();
                                return Task.CompletedTask;
                            }
                        };

                        options.TokenValidationParameters.ValidateIssuer = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð¸Ð·Ð´Ð°ÑÐµÐ»Ñ
                        options.TokenValidationParameters.ValidateAudience = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð¿Ð¾Ð»ÑÑÐ°ÑÐµÐ»Ñ
                        options.TokenValidationParameters.ValidateLifetime = true; // Ð¿ÑÐ¾Ð²ÐµÑÐºÐ° Ð½Ðµ Ð¿ÑÐ¾ÑÑÑ
                        options.TokenValidationParameters.RequireSignedTokens = true; // ÐµÑÑÑ Ð»Ð¸ Ð²Ð°Ð»Ð¸Ð´Ð½Ð°Ñ Ð¿Ð¾Ð´Ð¿Ð¸ÑÑ Ð¸Ð·Ð´Ð°ÑÐµÐ»Ñ
                    });
                    services.AddAuthentication(options =>
                        { 
                            options.DefaultSignInScheme = "Cookie"; 
                            options.DefaultChallengeScheme = "Passport"; 
                            options.DefaultScheme = "Cookie";
                        })
                        .AddCookie("Cookie", options =>
                        {
                            // NOTE: ÐÑÑÑÑ Ñ ÐºÑÐºÐ¸ Ð±ÑÐ´ÐµÑ Ð¸Ð¼Ñ, ÐºÐ¾ÑÐ¾ÑÐ¾Ðµ ÑÐ°ÑÑÐ¸ÑÑÐ¾Ð²ÑÐ²Ð°ÐµÑÑÑ Ð½Ð° ÑÑÑÐ°Ð½Ð¸ÑÐµ Â«DecodeÂ»
                            options.Cookie.Name = "PhotosApp.Auth";
                            // NOTE: ÐÑÐ»Ð¸ Ð½Ðµ Ð·Ð°Ð´Ð°ÑÑ Ð·Ð´ÐµÑÑ Ð¿ÑÑÑ Ð´Ð¾ Ð¾Ð±ÑÐ°Ð±Ð¾ÑÑÐ¸ÐºÐ° logout, ÑÐ¾ Ð² ÑÑÐ¾Ð¼ Ð¾Ð±ÑÐ°Ð±Ð¾ÑÑÐ¸ÐºÐµ
                            // Ð±ÑÐ´ÐµÑ Ð¸Ð³Ð½Ð¾ÑÐ¸ÑÐ¾Ð²Ð°ÑÑÑÑ ÑÐµÐ´Ð¸ÑÐµÐºÑ Ð¿Ð¾ Ð½Ð°ÑÑÑÐ¾Ð¹ÐºÐµ AuthenticationProperties.RedirectUri
                            options.LogoutPath = "/Passport/Logout";
                        });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Exception");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

            app.UseSerilogRequestLogging();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Photos}/{action=Index}/{id?}");
                //endpoints.MapRazorPages();
            });
        }
    }
}
