using Microsoft.EntityFrameworkCore;

namespace PhotosService.Data
{
    public class PhotosDbContext : DbContext
    {
        public PhotosDbContext(DbContextOptions<PhotosDbContext> options)
            : base(options)
        {
        }

        public DbSet<PhotoEntity> Photos { get; set; }
    }
}
