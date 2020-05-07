using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PhotosService.Data
{
    public static class PhotosDataExtensions
    {
        public static void PrepareDB(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        scope.ServiceProvider.GetRequiredService<PhotosDbContext>().Database.Migrate();

                        var photosDbContext = scope.ServiceProvider.GetRequiredService<PhotosDbContext>();
                        photosDbContext.SeedWithSamplePhotosAsync().Wait();
                    }
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occurred while migrating or seeding the database.");
                }
            }
        }

        private static async Task SeedWithSamplePhotosAsync(this PhotosDbContext dbContext)
        {
            dbContext.Photos.RemoveRange(dbContext.Photos);
            await dbContext.SaveChangesAsync();

            var photos = new[]
            {
                new PhotoEntity
                {
                     Id = new Guid("c92bff2b-d13b-4ffa-86ab-99b323ac47c9"),
                     Title = "Собор, Кристина",
                     FileName = "101.jpg",
                     OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },
                new PhotoEntity
                {
                     Id = new Guid("4c6979c1-6d41-47ca-82bd-f54312e94efe"),
                     Title = "Арка, Кристина",
                     FileName = "102.jpg",
                     OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },
                new PhotoEntity
                {
                     Id = new Guid("e1e87b85-4189-4ac6-b025-0af471b0984e"),
                     Title = "Дом, Кристина",
                     FileName = "103.jpg",
                     OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },
                new PhotoEntity
                {
                     Id = new Guid("dc8f2662-d47f-434c-b3e3-5894c182523e"),
                     Title = "Парк, Кристина",
                     FileName = "104.jpg",
                     OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },
                new PhotoEntity
                {
                    Id = new Guid("db795812-257d-4f52-8c79-054d8de5735f"),
                    Title = "С возвышения, Кристина",
                    FileName = "105.jpg",
                    OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },
                new PhotoEntity
                {
                    Id = new Guid("9b6f47cb-4018-4e13-8df4-6c6aa270d06b"),
                    Title = "Застройка, Кристина",
                    FileName = "106.jpg",
                    OwnerId = "dcaec9ce-91c9-4105-8d4d-eee3365acd82"
                },

                new PhotoEntity
                {
                     Id = new Guid("55921bb1-a38d-4bb2-9c5e-c8a591dcbef1"),
                     Title = "Аттракционы, Вики",
                     FileName = "201.jpg",
                     OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                },
                new PhotoEntity
                {
                     Id = new Guid("da5ba0b9-fc6c-492d-aebd-d10d7ae0f955"),
                     Title = "Площадь, Вики",
                     FileName = "202.jpg",
                     OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                },
                new PhotoEntity
                {
                     Id = new Guid("26cae583-26f7-47e1-b3fd-207da500728e"),
                     Title = "Собор, Вики",
                     FileName = "203.jpg",
                     OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                },
                new PhotoEntity
                {
                     Id = new Guid("785c4bd0-e7a2-4c31-98f6-342968112ef3"),
                     Title = "Всадник, Вики",
                     FileName = "204.jpg",
                     OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                },
                new PhotoEntity
                {
                    Id = new Guid("f3729380-5e06-4b04-917a-1760edf81ed7"),
                    Title = "Госпиталь, Вики",
                    FileName = "205.jpg",
                    OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                },
                new PhotoEntity
                {
                    Id = new Guid("f1fc7f53-f4b6-4bfd-9f7f-3463a0d8fdac"),
                    Title = "Улица, Вики",
                    FileName = "206.jpg",
                    OwnerId = "a83b72ed-3f99-44b5-aa32-f9d03e7eb1fd"
                }
            };

            dbContext.Photos.AddRange(photos);
            await dbContext.SaveChangesAsync();
        }
    }
}
