using System.ComponentModel.DataAnnotations;

namespace JwtAuthenticationManager.Models
{
    public class AuthenticationRequest
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
