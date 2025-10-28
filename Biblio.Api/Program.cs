using Biblio.Api.Middleware;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;
var services = builder.Services;

// DbContext
var conn = configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\MSSQLLocalDB;Database=BiblioDb;Trusted_Connection=True;";
services.AddDbContext<BiblioDbContext>(opts => opts.UseSqlServer(conn));

// Identity
services.AddIdentityCore<AppUser>(options =>
{
 options.Password.RequireNonAlphanumeric = false;
 options.Password.RequireUppercase = false;
 options.Password.RequiredLength =6;
})
 .AddRoles<IdentityRole>()
 .AddEntityFrameworkStores<BiblioDbContext>();

// JWT
var jwtSection = configuration.GetSection("Jwt");
// Try environment variable first, then config (appsettings/usersecrets)
var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY") ?? jwtSection.GetValue<string>("Key");
if (string.IsNullOrWhiteSpace(jwtKey)) throw new InvalidOperationException("JWT key not configured. Set via User Secrets or environment variable 'JWT__KEY'.");
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "Biblio.Api";
var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.RequireHttpsMetadata = true;
 options.SaveToken = true;
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidIssuer = jwtIssuer,
 ValidateAudience = false,
 IssuerSigningKey = signinKey,
 ValidateIssuerSigningKey = true,
 ValidateLifetime = true,
 ClockSkew = TimeSpan.FromSeconds(30)
 };
});

services.AddAuthorization();

// CORS: only allow the desktop app origin if configured
var allowedOrigin = configuration.GetValue<string>("Desktop:AllowedOrigin");
if (!string.IsNullOrWhiteSpace(allowedOrigin))
{
 services.AddCors(opts => opts.AddPolicy("DesktopOnly", p => p.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod()));
}

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
 // Add JWT Bearer support to Swagger
 var securityScheme = new OpenApiSecurityScheme
 {
 Name = "Authorization",
 Description = "Enter 'Bearer' [space] and then your valid token",
 In = ParameterLocation.Header,
 Type = SecuritySchemeType.Http,
 Scheme = "bearer",
 BearerFormat = "JWT",
 Reference = new OpenApiReference
 {
 Type = ReferenceType.SecurityScheme,
 Id = "Bearer"
 }
 };
 options.AddSecurityDefinition("Bearer", securityScheme);

 var securityRequirement = new OpenApiSecurityRequirement
 {
 { securityScheme, new[] { "Bearer" } }
 };
 options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// Global error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}
else
{
 app.UseHsts();
}

app.UseHttpsRedirection();

if (!string.IsNullOrWhiteSpace(allowedOrigin)) app.UseCors("DesktopOnly");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
