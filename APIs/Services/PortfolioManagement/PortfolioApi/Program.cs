using JwtAuthenticationManager;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Model;
using PortfolioApi.Infrastructure;
using PortfoliApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Adding Authentication
builder.Services.AddCustomJwtAuthentication();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<ITaxFeeRepository, TaxFeeRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
