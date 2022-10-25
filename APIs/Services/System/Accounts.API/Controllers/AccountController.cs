using Accounts.API.EmailService;
using Accounts.API.Models;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg;
using System.Net.WebSockets;

namespace Accounts.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtTokenHandler _jwtTokenHandler;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public AccountController(JwtTokenHandler jwtTokenHandler,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
            , IEmailSender emailService, 
            IConfiguration configuration
            )
        {
            _jwtTokenHandler = jwtTokenHandler;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse?>> Authenticate([FromBody] AuthenticationRequest authenticationRequest)
        {
            if(string.IsNullOrEmpty(authenticationRequest.UserName)|| string.IsNullOrEmpty(authenticationRequest.Password))
                return Unauthorized();
            AuthenticationResponse authenticationResponse = null;
            var user = await _userManager.FindByNameAsync(authenticationRequest.UserName);
            if (user != null &&
                await _userManager.CheckPasswordAsync(user, authenticationRequest.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                authenticationResponse = await _jwtTokenHandler.GenerateJwtToken(authenticationRequest,user,roles.ToList<string>());
                if (authenticationResponse == null) 
                    return Unauthorized();
                else
                    return authenticationResponse;
            }
            else
                return Unauthorized();

        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user == null || !(await _userManager.CheckPasswordAsync(user.Result, model.OldPassword)))
                {
                    return BadRequest();
                }
                else
                {
                    var result   = await _userManager.ChangePasswordAsync(user.Result, model.OldPassword, model.NewPassword);
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Change password failed! Please check user details and try again." });
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Change passwor successfull!"
                });
            }

            // If we got this far, something failed, redisplay form
            return BadRequest();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return BadRequest();
                }
                var emailTo = new List<EmailAddress>();
                emailTo.Add(new EmailAddress() { DisplayName = user.UserName, Address = user.Email });
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var confirmationLink =
                    _configuration.GetSection("EmailConfiguration").Get<MailSettings>().HostName
                    + Url.Action(nameof(VerifyEmail), "Account", new { userId = user.Id, code });
                Task<string> content = GetContent(confirmationLink);
                await _emailSender.SendEmailAsync(new MailData(emailTo, "Confirm your email", content.Result, null));

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Email reset passwor send to your email!"
                });
            }

            // If we got this far, something failed, redisplay form
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            else // generation of the email token
            {
                var _code = _userManager.GenerateEmailConfirmationTokenAsync(user);
                var value = JsonConvert.SerializeObject(new { userId = user.Id, _code = _code });                
                var emailTo = new List<EmailAddress>();
                emailTo.Add(new EmailAddress() { DisplayName = user.UserName, Address = user.Email });
                
                var confirmationLink = 
                    _configuration.GetSection("EmailConfiguration").Get<MailSettings>().HostName
                    + Url.Action(nameof(VerifyEmail), "Account", new { userId = user.Id, _code.Result });

                Task<string> content = GetContent(confirmationLink);

                await _emailSender.SendEmailAsync(new MailData(emailTo,"Confirm your email",content.Result,null));
                

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "User created successfully!",
                    Data = value
                });
            }

        }

        private async Task<String> GetContent(string link)
        {
            string html = string.Format("<a href =\"{0}\"> Verify email </a>",link);
            return html;
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId,string Result)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();
            var result = await _userManager.ConfirmEmailAsync(user, Result);
            if(result.Succeeded)
            {
                // Select the user, and then add the admin role to the user                 
                if (!await _userManager.IsInRoleAsync(user, "VISITOR"))
                {
                    //_logger.LogInformation("Adding sysadmin to Admin role");
                    try
                    {
                       // await _roleManager.CreateAsync(new IdentityRole() { Id = "MD_PORTFOLIO_ADD", Name = "MD_PORTFOLIO_ADD" });
                        var userResult = await _userManager.AddToRoleAsync(user, "VISITOR");
                    }
                    catch (Exception exx)
                    {
                        throw exx;
                    }
                    
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Confirn your email successfully!"                    
                });
            }    
            else
                return BadRequest();
        }

        [Authorize]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return Ok(new Response
            {
                Status = "Success",
                Message = "Logout successfully!"
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetListUser()
        {
            var userExists = _userManager.Users;
            if (userExists != null)
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Not found data!" });
            var data = JsonConvert.SerializeObject(userExists);
            return Ok(new Response { Status = "Success", Message = "User created successfully!", Data = data });
        }
    }
}
