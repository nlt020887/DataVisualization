using Microsoft.AspNetCore.Identity;

namespace Accounts.API.Models
{
    public class UserInfoResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }    
        public DateTime? CreatedDate { get; set; }   
        public DateTime? ConfirmEmailDate { get; set; }  
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public Boolean IsEnabled { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public Boolean IsNewsFeed { get; set; }
        public Boolean EmailConfirmed { get; set; }
        public List<ApplicationRole> Roles { get; set; }

    }
}
