using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotosService.Data
{
    public interface IPhotosRepository
    {
        Task<IEnumerable<PhotoEntity>> GetPhotosAsync(string ownerId);
        Task<PhotoEntity> GetPhotoMetaAsync(Guid id);
        Task<PhotoContent> GetPhotoContentAsync(Guid id);
        Task<bool> AddPhotoAsync(string title, string ownerId, byte[] content);
        Task<bool> UpdatePhotoAsync(PhotoEntity photo);
        Task<bool> DeletePhotoAsync(PhotoEntity photo);
    }
}
