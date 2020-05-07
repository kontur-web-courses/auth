using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotosApp.Data
{
    public interface IPhotosRepository
    {
        Task<PhotoEntity> GetPhotoMetaAsync(Guid id);
        Task<PhotoContent> GetPhotoContentAsync(Guid id);
        Task<IEnumerable<PhotoEntity>> GetPhotosAsync(string ownerId);
        Task<bool> AddPhotoAsync(string title, string ownerId, byte[] content);
        Task<bool> UpdatePhotoAsync(PhotoEntity photo);
        Task<bool> DeletePhotoAsync(PhotoEntity photo);
    }
}