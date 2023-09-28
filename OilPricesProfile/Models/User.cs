using Microsoft.AspNetCore.Identity;

namespace OilPricesProfile.Models
{
    public class User : IdentityUser
    {
    }

    public class UserRole : IdentityRole
    {
        // Additional role properties if needed
    }
}