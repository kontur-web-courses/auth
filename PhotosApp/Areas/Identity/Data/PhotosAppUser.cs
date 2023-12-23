using Microsoft.AspNetCore.Identity;

namespace PhotosApp.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the PhotosAppUser class
    public class PhotosAppUser : IdentityUser
    {
        public bool Paid { get; set; }
    }
}
