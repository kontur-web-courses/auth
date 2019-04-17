using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PhotoApp.Data;

namespace PhotoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.PrepareDB();
            host.Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
