using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoApp.Models
{
    public class EditPhotoModel
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public Guid Id { get; set; }
    }
}
