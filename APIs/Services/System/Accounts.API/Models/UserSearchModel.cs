namespace Accounts.API.Models
{
    public class UserSearchModel
    {
        public string Keyword { get; set; }
        public Boolean? IsEmailConfirm { get; set; }
        public Boolean? IsEnable { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
