using System.ComponentModel.DataAnnotations;

namespace PhotosApp.Clients.Models
{
    public class PhotoToUpdateDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
    }
}
