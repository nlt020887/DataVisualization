namespace Accounts.API.Models
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }

    }
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewConfirmPassword { get; set; }
    }
}
