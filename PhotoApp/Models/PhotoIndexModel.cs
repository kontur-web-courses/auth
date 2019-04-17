using System.Collections.Generic;

namespace PhotoApp.Models
{
    public class PhotoIndexModel
    {
        public IEnumerable<Photo> Photos { get; private set; }
            = new List<Photo>();

        public PhotoIndexModel(List<Photo> photos)
        {
           Photos = photos;
        }
    }
}
