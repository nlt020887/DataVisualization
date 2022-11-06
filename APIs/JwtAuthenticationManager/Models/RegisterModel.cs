using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthenticationManager.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "PasswordConfirm is required")]
        public string? PasswordConfirm { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string? PhoneNumber{ get; set; }

        public string Address { get; set; }

        public bool IsNewsFeed { get; set; } = false;
        public string FullName { get; set; }
        

    }
}
