using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PhotosApp.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the PhotosAppUser class
    public class PhotosAppUser : IdentityUser
    {
        public bool Paid { get; set; }
    }
}
