using Accounts.API.DbContext;
using Accounts.API.EmailService;
using Accounts.API.Models;
using JwtAuthenticationManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
{
    config.Password.RequiredLength = 4;
    config.Password.RequireDigit = false;
    config.Password.RequireNonAlphanumeric = false;
    config.Password.RequireUppercase = false;
    config.SignIn.RequireConfirmedEmail = true;      
    
}).AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
//Emailkit
var emailConfig = builder.Configuration
                .GetSection("EmailConfiguration")
                .Get<MailSettings>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Adding Authentication
builder.Services.AddCustomJwtAuthentication();
//Add Jwt Token handler.
builder.Services.AddSingleton<JwtTokenHandler>();

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
