using JwtAuthenticationManager;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Model;
using PortfolioApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Adding Authentication
builder.Services.AddCustomJwtAuthentication();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
