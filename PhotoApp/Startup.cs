using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoApp.Data;
using PhotoApp.Models;

namespace PhotoApp
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        static Startup()
        {
            ConfigureAutoMapper();
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(Configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
            });

            services.AddDbContext<PhotosDbContext>(o =>
                o.UseSqlite(Configuration.GetConnectionString("PhotosDbContextConnection")));
            // NOTE: Вместо Sqlite можно использовать LocalDB от Microsoft или другой SQL Server
            //services.AddDbContext<PhotosDbContext>(o =>
            //    o.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=PhotoApp;Trusted_Connection=True;"));

            services.AddScoped<IPhotoRepository, PhotoRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            app.UseStatusCodePagesWithRedirects("/Error/StatusCode/{0}");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(exceptionApp =>
                {
                    exceptionApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        var exception = context.Features.Get<IExceptionHandlerFeature>().Error;
                        logger.LogError(exception, "Unhandled Error");
                    });
                });
            }

            if (!env.IsDevelopment()) app.UseHsts();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Photo}/{action=Index}/{id?}");
            });
        }

        private static void ConfigureAutoMapper()
        {
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<PhotoEntity, Photo>().ReverseMap();

                cfg.CreateMap<AddPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                cfg.CreateMap<EditPhotoModel, PhotoEntity>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();
        }
    }
}
