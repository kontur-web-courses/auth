using System.ComponentModel.DataAnnotations;

namespace PhotosService.Models
{
    public class PhotoToUpdateDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
    }
}
