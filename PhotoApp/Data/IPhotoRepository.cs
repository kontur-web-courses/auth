using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoApp.Data
{
    public interface IPhotoRepository
    {
        Task<bool> IsPhotoExistAsync(Guid id);
        Task<PhotoEntity> GetPhotoAsync(Guid id);
        Task<IEnumerable<PhotoEntity>> GetPhotosAsync(string ownerId);
        Task<bool> IsPhotoOwnerAsync(Guid id, string ownerId);
        Task AddPhotoAsync(PhotoEntity photo);
        Task UpdatePhotoAsync(PhotoEntity photo);
        Task DeletePhotoAsync(PhotoEntity photo);
        Task<bool> SaveAsync();
    }
}