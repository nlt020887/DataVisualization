using Microsoft.AspNetCore.Identity;

namespace Accounts.API.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
    }
}
