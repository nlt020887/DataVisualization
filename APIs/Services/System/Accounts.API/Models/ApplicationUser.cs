using Microsoft.AspNetCore.Identity;

namespace Accounts.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsNewsFeed{ get; set; } =false;        
        public string Company { get; set; }
        public string TaxCode { get; set; }
        public string FullName { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Today;
        public DateTime? ConfirmEmailDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedUser { get; set; }
    }
}
