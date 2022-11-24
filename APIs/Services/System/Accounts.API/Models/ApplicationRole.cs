using Microsoft.AspNetCore.Identity;

namespace Accounts.API.Models
{
    public class ApplicationRole : IdentityRole
    {
        public String? Description { get; set; }
    }
}
