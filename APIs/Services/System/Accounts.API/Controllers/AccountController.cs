using Accounts.API.DbContext;
using Accounts.API.EmailService;
using Accounts.API.Models;
using JwtAuthenticationManager;
using JwtAuthenticationManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;

namespace Accounts.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtTokenHandler _jwtTokenHandler;
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public AccountController(JwtTokenHandler jwtTokenHandler,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager
            , IEmailSender emailService, 
            IConfiguration configuration,
            ApplicationDbContext _dbContext
            )
        {
            _jwtTokenHandler = jwtTokenHandler;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailService;
            _configuration = configuration;
            dbContext = _dbContext;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse?>> Authenticate([FromBody] AuthenticationRequest authenticationRequest)
        {
            if(string.IsNullOrEmpty(authenticationRequest.UserName)|| string.IsNullOrEmpty(authenticationRequest.Password))
                return Unauthorized();
            AuthenticationResponse authenticationResponse = null;
            var user = await _userManager.FindByNameAsync(authenticationRequest.UserName);
            if (user == null)
                return Unauthorized();
            else
            if (!user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status102Processing, new Response { Status = "Error", Message = "Please check your email to confirm your account!" });
            }
            if (user != null &&
                await _userManager.CheckPasswordAsync(user, authenticationRequest.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                authenticationResponse = await _jwtTokenHandler.GenerateJwtToken(authenticationRequest,user,roles.ToList<string>());
                if (authenticationResponse == null)
                    return Unauthorized();
                else
                {
                    await _userManager.SetAuthenticationTokenAsync(user, "Jwt", "JwtToken", authenticationResponse.JwtToken);
                    return authenticationResponse;
                }
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
            if(string.IsNullOrEmpty(model.Email))
                return StatusCode(StatusCodes.Status204NoContent, new Response { Status = "Error", Message = "Email can not empty!" });

            if (string.IsNullOrEmpty(model.Password) || !model.Password.Equals(model.PasswordConfirm))
                return StatusCode(StatusCodes.Status204NoContent, new Response { Status = "Error", Message = "Invaild confirmation password!" });

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                IsNewsFeed = model.IsNewsFeed,
                Address = model.Address,
                Company  = model.Company,
                CreatedDate = DateTime.Now,
                FullName = model.FullName,
                TaxCode = model.TaxCode
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
                    Message = "User created successfully!  ",
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

            string jwt = HttpContext.Request.Headers.Authorization;
            return Ok(new Response
            {
                Status = "Success",
                Message = "Logout successfully!"
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetListUser(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var qry = _userManager.Users.Where(u => u.Id != String.Empty)
                        .Select(u => new UserInfoResponse
                        {
                            UserId = u.Id,
                            UserName = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            Address = u.Address,
                            Company = u.Company,
                            TaxCode = u.TaxCode,
                            Email = u.Email,
                            ConfirmEmailDate = u.ConfirmEmailDate,
                            EmailConfirmed = u.EmailConfirmed
                        });


                //var values =  new PaginatedList<UserInfoResponse>(qry,qry.Count,pageNumber, pageSize);
                //IEnumerable<UserInfoResponse> products = ;

                PaginatedList<UserInfoResponse> a = await PaginatedList<UserInfoResponse>.CreateAsync(qry, pageNumber, pageSize);
                ResutlPagingModel<UserInfoResponse> resutlPagingModel = new ResutlPagingModel<UserInfoResponse>();
                resutlPagingModel.TotalPages = a.TotalPages;
                resutlPagingModel.PageIndex = a.PageIndex;
                resutlPagingModel.Items = a.ToList<UserInfoResponse>();
                return Ok(new Response { Status = "Success", Message = "User created successfully!", Data = JsonConvert.SerializeObject(resutlPagingModel) });
                
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserById(string userId)
        {

            try
            {
                if(string.IsNullOrEmpty(userId))
                    return Ok(new Response { Status = "Error", Message = "User id can't empty!", Data = null });

                var qry = _userManager.Users.Where(u => u.Id == userId)
                         .Select(u => new UserInfoResponse
                         {
                             UserId = u.Id,
                             UserName = u.UserName,
                             PhoneNumber = u.PhoneNumber,
                             Address = u.Address,
                             Company = u.Company,
                             TaxCode = u.TaxCode,
                             Email = u.Email,
                             ConfirmEmailDate = u.ConfirmEmailDate,
                             EmailConfirmed = u.EmailConfirmed
                         });
                var userInfo = qry.First<UserInfoResponse>();
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return BadRequest();

               IList<string> roleNames =  await _userManager.GetRolesAsync(user);
                
                var roles = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToList();
                if (roles != null && roles.Count > 0)
                    userInfo.Roles = roles;

                if (qry!=null && qry.Count()>0)
                    return Ok(new Response { Status = "Success", Message = "User created successfully!", Data = JsonConvert.SerializeObject(userInfo)});
                  else
                    return Ok(new Response { Status = "Error", Message = "User not found!", Data = null});
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetRoles()
        {

            try
            {
                var Roles = _roleManager.Roles;
                
                if (Roles != null && Roles.Count() > 0)
                    return Ok(new Response { Status = "Success", Message = "User created successfully!", Data = JsonConvert.SerializeObject(Roles) });
                else
                    return Ok(new Response { Status = "Error", Message = "User not found!", Data = null });
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo(UserInfoResponse user)
        {

            try
            {
                if (user == null ||  string.IsNullOrEmpty(user.UserId))
                    return Ok(new Response { Status = "Error", Message = "User info is not null!", Data = null });

                var userCheck = await _userManager.FindByIdAsync(user.UserId);
                if (userCheck == null) 
                    return Ok(new Response { Status = "Error", Message = "User does not exist!", Data = null });

                userCheck.FullName = user.FullName;
                userCheck.Address = user.Address;
                userCheck.PhoneNumber = user.PhoneNumber;
                userCheck.IsNewsFeed = user.IsNewsFeed;

                IList<string> roleNames = await _userManager.GetRolesAsync(userCheck);
                ///check if the user currently has any roles
                var currentRoles = await _userManager.GetRolesAsync(userCheck);
                ///remove user from current roles, if any
                IdentityResult removeResult = await _userManager.RemoveFromRolesAsync(userCheck, currentRoles.ToArray());             
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove user roles");
                    return BadRequest(ModelState);
                }
                //var roles = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToList();

                var qry = user.Roles.Where(o => 1 == 1)
                         .Select(o => o.Id);
                ///assign user to the new roles
                IdentityResult addResult = await _userManager.AddToRolesAsync(userCheck, qry.ToArray());

                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add user roles");
                    return BadRequest(ModelState);
                }
                return Ok(new Response { Status = "Success", Message = "Update user successfully!", Data = null });
             
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateRole(RoleInfo roleInfo)
        {
            try
            {
                if (await _roleManager.FindByIdAsync(roleInfo.RoleId) ==null)
                {
                    await _roleManager.CreateAsync(new ApplicationRole() { Id = roleInfo.RoleId, Name = roleInfo.RoleName, Description = roleInfo. Description});
                }
                else
                    return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message =  "Role is exist"});

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Confirn your email successfully!"
                });

            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = EX.Message });
            }
        }

        private async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
            
        }
    }
}
