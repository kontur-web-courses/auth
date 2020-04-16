using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PhotosApp.Models
{
    public class AddPhotoModel
    {
        public List<IFormFile> Files { get; set; } = new List<IFormFile>();

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
    }
}
