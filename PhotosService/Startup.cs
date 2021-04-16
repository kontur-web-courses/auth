using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using PhotosService.Data;
using PhotosService.Models;
using Serilog;

namespace PhotosService
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            var connectionString = configuration.GetConnectionString("PhotosDbContextConnection")
                ?? "Data Source=PhotosService.db";
            services.AddDbContext<PhotosDbContext>(o => o.UseSqlite(connectionString));

            services.AddScoped<IPhotosRepository, LocalPhotosRepository>();

            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<PhotoEntity, PhotoDto>().ReverseMap();
            }, new System.Reflection.Assembly[0]);

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:7001";
                    options.Audience = "photos_service";
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
