using System;

namespace PhotosService.Models
{
    public class PhotoDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string OwnerId { get; set; }
    }
}