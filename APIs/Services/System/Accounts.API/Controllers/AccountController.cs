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
        public async Task<ActionResult> Authenticate([FromBody] AuthenticationRequest authenticationRequest)
        {
            if(string.IsNullOrEmpty(authenticationRequest.UserName)|| string.IsNullOrEmpty(authenticationRequest.Password))
               return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            AuthenticationResponse authenticationResponse = null;
            var user = await _userManager.FindByNameAsync(authenticationRequest.UserName);
            if (user == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
            else
            if (!user.EmailConfirmed)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản chưa được kích hoạt, vui lòng kiểm tra Email!" });
            }
            else
            if (!user.IsEnabled.Value)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản đã bị khóa, Vui lòng liên hệ quản trị viên!" });
            }

            if (user != null &&
                await _userManager.CheckPasswordAsync(user, authenticationRequest.Password))
            {
                var roles = await _userManager.GetRolesAsync(user);
                authenticationResponse = await _jwtTokenHandler.GenerateJwtToken(authenticationRequest,user,roles.ToList<string>());
                if (authenticationResponse == null)
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
                else
                {
                    HttpContext.Response.Cookies.Append("jwtToken", authenticationResponse.JwtToken,
                    new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = false,
                        Secure = false,
                        IsEssential = true,
                        SameSite = SameSiteMode.None
                    });
                    await _userManager.SetAuthenticationTokenAsync(user, "Jwt", "JwtToken", authenticationResponse.JwtToken);
                    
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "",Data = JsonConvert.SerializeObject(authenticationResponse)}); ;
                }
            }
            else
              return  StatusCode(StatusCodes.Status200OK, new Response{ Status = "Error", Message = "Tên đăng nhập hoặc mật khẩu không đúng!" });

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
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Thay đổi mật khẩu thất bại! Vui lòng kiểm tra và thử lại!" });
                }
                else
                {
                    var result   = await _userManager.ChangePasswordAsync(user.Result, model.OldPassword, model.NewPassword);
                    if (!result.Succeeded)
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Thay đổi mật khẩu thất bại! Vui lòng kiểm tra và thử lại" });
                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Thay đổi mật khẩu thành công!"
                });
            }

            // If we got this far, something failed, redisplay form
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Thay đổi mật khẩu thất bại! Vui lòng kiểm tra và thử lại!\"" }); 
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null || !user.IsEnabled.Value)
                    {
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản của bạn đã bị khóa, Vui lòng liên hệ quản trị viên!" });
                    }
                
                var emailTo = new List<EmailAddress>();
                emailTo.Add(new EmailAddress() { DisplayName = user.UserName, Address = user.Email });
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var confirmationLink =
                    _configuration.GetSection("EmailConfiguration").Get<MailSettings>().HostName
                    + Url.Action("ResetPassword", "Account", new { userId = user.Id, code });
                 confirmationLink = confirmationLink.Replace("api/","");
                Task<string> content = GetContentResetPass(confirmationLink,user.Email);
                await _emailSender.SendEmailAsync(new MailData(emailTo, "Xác thực lấy lại mật khẩu Growth Focused Funds!", content.Result, null));

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Đã gửi email, Vui lòng kiểm tra Email của bạn!",
                    Data = code
                });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = ex.Message});
                }
            }
            
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Lỗi không xác định!" });
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if(string.IsNullOrEmpty(model.Email))
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Email không được trống!" });

            if (string.IsNullOrEmpty(model.Password) || !model.Password.Equals(model.PasswordConfirm))
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Mập khẩu không khớp!" });
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Email đã tồn tại đăng ký!" });
            userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tên đăng nhập đã tồn tại!" });

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
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Đăng ký tài khoản không thành công! Vui lòng thử lại" });
            else // generation of the email token
            {
                var _code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var value = JsonConvert.SerializeObject(new { userId = user.Id, _code = _code });                
                var emailTo = new List<EmailAddress>();
                emailTo.Add(new EmailAddress() { DisplayName = user.UserName, Address = user.Email });

                var confirmationLink =
                    _configuration.GetSection("EmailConfiguration").Get<MailSettings>().HostName
                    + Url.Action("VerifyEmail", "Account", new { userId = user.Id, Result= _code });
                confirmationLink = confirmationLink.Replace("api/", "");
                Task<string> content = GetContent(confirmationLink,user.Email);

                await _emailSender.SendEmailAsync(new MailData(emailTo, "Xác thực đăng ký tài khoản Growth Focused Funds!", content.Result,null));
                

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Đăng ký tài khoản thành công!",
                    Data = value
                });
            }

        }

        private async Task<String> GetContent(string link, string email)
        {
            string content = @" <span>
            Xin chào {0} ,<br>
            Bạn vừa có yêu cầu đăng ký tài khoản hệ thống Growth Funds system.<br>
            Link xác thực đăng ký: {1}. </br>";
            string html = string.Format(" <a href =\"{0}\"> Click xác thực email</a>",link);
            return string.Format(content, email, html);
            
        }

        private async Task<String> GetContentResetPass(string link, string email)
        {
            string content = @" <span>
            Xin chào {0} ,<br>
            Bạn vừa có yêu cầu đặt lại mật khẩu cho tài khoản hệ thống Growth Funds system.<br>
            Link đặt lại mật khẩu: {1}. </br>";
            string html = string.Format(" <a href =\"{0}\"> Click link.</a>", link);
            return string.Format(content, email, html);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailModel verifyEmailModel)
        {
            var user = await _userManager.FindByIdAsync(verifyEmailModel.UserId);
            if (user == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản không tồn tại." });
            var result = await _userManager.ConfirmEmailAsync(user, verifyEmailModel.Token);
            if (result.Succeeded)
            {
                user.ConfirmEmailDate = DateTime.Now;
                await _userManager.UpdateAsync(user);
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
                        return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = exx.Message });
                    }

                }

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Xác thực email thành công!"
                });
            }
            else
            {
                string err = string.Empty;
                if(result.Errors!=null && result.Errors.Count()>0)
                {
                    var error = result.Errors.First();
                    err = error?.Description;   
                }
              return  StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = err});
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if(string.IsNullOrEmpty(resetPasswordModel.Password) || !string.Equals(resetPasswordModel.Password,resetPasswordModel.ConfirmPassword))
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Mật khẩu không khớp!" });            
            var user = await _userManager.FindByIdAsync(resetPasswordModel.UserId);
            if (user == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản không tồn tại!" });
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                string mes = "Đặt lại mật khẩu không thành công!";
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                    mes = error.Description;
                }
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = mes });
                
            }
            return Ok(new Response
            {
                Status = "Success",
                Message = "Đặt lại mật khẩu thành công!"
            });
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GetListUser(UserSearchModel model)
        {
            try
            {
                var qry = _userManager.Users.Where(u => u.Id != String.Empty && (string.IsNullOrEmpty(model.Keyword) ||
                u.NormalizedUserName == model.Keyword.Trim().ToUpper()
                || u.NormalizedEmail == model.Keyword.Trim().ToUpper())
                && (u.IsEnabled == model.IsEnable)
                && (model.IsEmailConfirm==null ||  u.EmailConfirmed == model.IsEmailConfirm)
                )
                       .Select(u => new UserInfoResponse
                       {
                           UserId = u.Id,
                           UserName = u.UserName,
                           FullName = u.FullName,
                           PhoneNumber = u.PhoneNumber,
                           Address = u.Address,
                           Company = u.Company,
                           TaxCode = u.TaxCode,
                           CreatedDate = u.CreatedDate,
                           UpdatedDate = u.UpdatedDate,
                           UpdatedUser = u.UpdatedUser,
                           IsEnabled = u.IsEnabled,
                           Email = u.Email,
                           ConfirmEmailDate = u.ConfirmEmailDate,
                           EmailConfirmed = u.EmailConfirmed,
                           IsNewsFeed = u.IsNewsFeed
                       });


                //var values =  new PaginatedList<UserInfoResponse>(qry,qry.Count,pageNumber, pageSize);
                //IEnumerable<UserInfoResponse> products = ;

                PaginatedList<UserInfoResponse> a = await PaginatedList<UserInfoResponse>.CreateAsync(qry, model.PageNumber, model.PageSize);
                ResutlPagingModel<UserInfoResponse> resutlPagingModel = new ResutlPagingModel<UserInfoResponse>();
                resutlPagingModel.TotalPages = a.TotalPages;
                resutlPagingModel.PageIndex = a.PageIndex;
                resutlPagingModel.Items = a.ToList<UserInfoResponse>();
                return Ok(new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(resutlPagingModel) });
                
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserById(string userId)
        {

            try
            {
                if(string.IsNullOrEmpty(userId))
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Mã user không hợp lệ!" });                

                var qry = _userManager.Users.Where(u => u.Id == userId)
                         .Select(u => new UserInfoResponse
                         {
                             UserId = u.Id,
                             UserName = u.UserName,
                             FullName = u.FullName,
                             PhoneNumber = u.PhoneNumber,
                             Address = u.Address,
                             Company = u.Company,
                             TaxCode = u.TaxCode,
                             CreatedDate = u.CreatedDate,
                             UpdatedDate = u.UpdatedDate,
                             UpdatedUser = u.UpdatedUser,
                             IsEnabled = u.IsEnabled.Value,                             
                             Email = u.Email,
                             ConfirmEmailDate = u.ConfirmEmailDate,
                             EmailConfirmed = u.EmailConfirmed,
                             IsNewsFeed = u.IsNewsFeed                             
                         });
                var userInfo = qry.First<UserInfoResponse>();
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản không tồn tại!" });

                IList<string> roleNames =  await _userManager.GetRolesAsync(user);
                
                var roles = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToList();
                if (roles != null && roles.Count > 0)
                    userInfo.Roles = roles;

                if (qry!=null && qry.Count()>0)
                    return Ok(new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(userInfo)});
                  else
                    return Ok(new Response { Status = "Error", Message = "Tài khoản không tồn tại!", Data = null});
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = EX.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetFullListUser()
        {

            try
            {
              
                var qry = _userManager.Users.Where(u => u.IsEnabled==true)
                         .Select(u => new 
                         {                       
                             UserName = u.UserName,
                             FullName = u.FullName
                         });
               
                if (qry != null && qry.Count() > 0)
                    return Ok(new Response { Status = "Success", Message = "!", Data = JsonConvert.SerializeObject(qry) });
                else
                    return Ok(new Response { Status = "Success", Message = "Không có dữ liệu!", Data = null });
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = EX.Message });
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
                    return Ok(new Response { Status = "Success", Message = "", Data = JsonConvert.SerializeObject(Roles) });
                else
                    return Ok(new Response { Status = "Error", Message = "Danh sách quyền không tồn tại", Data = null });
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
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User Id không khợp lệ", Data = null });

                var userCheck = await _userManager.FindByIdAsync(user.UserId);
                if (userCheck == null)
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Tài khoản không tồn tại!", Data = null });

                userCheck.FullName = user.FullName;
                userCheck.PhoneNumber = user.PhoneNumber;
                userCheck.Email = user.Email;
                userCheck.Company = user.Company;
                userCheck.Address = user.Address;
                userCheck.UpdatedDate = DateTime.Now;
                userCheck.UpdatedUser = HttpContext.User.Identity.Name;
                userCheck.TaxCode= user.TaxCode;
                userCheck.IsEnabled = user.IsEnabled;
                userCheck.IsNewsFeed = user.IsNewsFeed;
                IdentityResult updateResult = await _userManager.UpdateAsync(userCheck);
                if (!updateResult.Succeeded)
                {
                    ModelState.AddModelError("", "Lỗi cập nhật thông tin tài khoản");
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Lỗi cập nhật thông tin tài khoản", Data = null });
                }

                IList<string> roleNames = await _userManager.GetRolesAsync(userCheck);
                ///check if the user currently has any roles
                var currentRoles = await _userManager.GetRolesAsync(userCheck);
                ///remove user from current roles, if any
                IdentityResult removeResult = await _userManager.RemoveFromRolesAsync(userCheck, currentRoles.ToArray());             
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to remove user roles");
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Failed to remove user roles", Data = null });
                }
                //var roles = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).ToList();

                var qry = user.Roles.Where(o => 1 == 1)
                         .Select(o => o.Id);
                ///assign user to the new roles
                IdentityResult addResult = await _userManager.AddToRolesAsync(userCheck, qry.ToArray());

                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add user roles");
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Failed to remove user roles", Data = null });
                }
                return Ok(new Response { Status = "Success", Message = "Cập nhật thông tin tài khoản thành công!", Data = null });
             
            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = EX.Message });
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
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message =  "The role is exist"});

                return Ok(new Response
                {
                    Status = "Success",
                    Message = "Tạo role thành công!"
                });

            }
            catch (Exception EX)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = EX.Message });
            }
        }

        private async Task<List<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
            
        }
    }
}
