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
var jwtKey = jwtSection.GetValue<string>("Key") ?? "ReplaceWithStrongKeyChangeInProduction";
var jwtIssuer = jwtSection.GetValue<string>("Issuer") ?? "Biblio.Api";
var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

services.AddAuthentication(options =>
{
 options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.RequireHttpsMetadata = false;
 options.SaveToken = true;
 options.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidIssuer = jwtIssuer,
 ValidateAudience = false,
 IssuerSigningKey = signinKey,
 ValidateIssuerSigningKey = true,
 ValidateLifetime = true
 };
});

services.AddAuthorization();

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

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
