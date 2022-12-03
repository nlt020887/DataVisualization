using Microsoft.AspNetCore.Identity;

namespace Accounts.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsNewsFeed{ get; set; } =false;        
        public String? Company { get; set; }
        public String? TaxCode { get; set; }
        public string FullName { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Today;
        public DateTime? ConfirmEmailDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public String? UpdatedUser { get; set; }
        public bool? IsEnabled { get; set; } = true;
    }
}
