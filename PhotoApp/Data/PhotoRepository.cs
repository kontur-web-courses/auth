using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhotoApp.Data
{
    public class PhotoRepository : IPhotoRepository, IDisposable
    {
        PhotosDbContext dbContext;

        public PhotoRepository(PhotosDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> IsPhotoExistAsync(Guid id)
        {
            return await dbContext.Photos.AnyAsync(p => p.Id == id);
        }

        public async Task<PhotoEntity> GetPhotoAsync(Guid id)
        {
            return await dbContext.Photos.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PhotoEntity>> GetPhotosAsync(string ownerId)
        {
            return await dbContext.Photos
                .Where(i => i.OwnerId == ownerId)
                .OrderBy(i => i.Title).ToListAsync();
        }

        public async Task<bool> IsPhotoOwnerAsync(Guid id, string ownerId)
        {
            return await dbContext.Photos.AnyAsync(p => p.Id == id && p.OwnerId == ownerId);
        }

        public async Task AddPhotoAsync(PhotoEntity photo)
        {
            await dbContext.Photos.AddAsync(photo);
        }

        public async Task UpdatePhotoAsync(PhotoEntity photo)
        {
            dbContext.Photos.Update(photo);
        }

        public async Task DeletePhotoAsync(PhotoEntity photo)
        {
            dbContext.Photos.Remove(photo);
        }

        public async Task<bool> SaveAsync()
        {
            return await dbContext.SaveChangesAsync() >= 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (dbContext != null)
                {
                    dbContext.Dispose();
                    dbContext = null;
                }
            }
        }
    }
}
