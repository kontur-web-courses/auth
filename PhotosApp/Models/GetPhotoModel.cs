namespace PhotosApp.Models
{
    public class GetPhotoModel
    {
        public Photo Photo { get; private set; }

        public GetPhotoModel(Photo photo)
        {
            Photo = photo;
        }
    }
}