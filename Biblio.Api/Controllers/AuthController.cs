using Biblio.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Biblio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
 private readonly UserManager<AppUser> _userManager;
 private readonly IConfiguration _config;

 public AuthController(UserManager<AppUser> userManager, IConfiguration config)
 {
 _userManager = userManager;
 _config = config;
 }

 public record LoginRequest(string Email, string Password);
 public record LoginResponse(string Token);

 [HttpPost("token")]
 public async Task<IActionResult> Token([FromBody] LoginRequest req)
 {
 var user = await _userManager.FindByEmailAsync(req.Email);
 if (user == null) return Unauthorized();
 var valid = await _userManager.CheckPasswordAsync(user, req.Password);
 if (!valid) return Unauthorized();

 var jwtSection = _config.GetSection("Jwt");
 var key = jwtSection.GetValue<string>("Key") ?? "ReplaceWithStrongKeyChangeInProduction";
 var issuer = jwtSection.GetValue<string>("Issuer") ?? "Biblio.Api";
 var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

 var claims = new List<Claim>
 {
 new Claim(JwtRegisteredClaimNames.Sub, user.Id),
 new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
 new Claim("fullName", user.FullName ?? string.Empty)
 };

 var roles = await _userManager.GetRolesAsync(user);
 foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

 var token = new JwtSecurityToken(
 issuer: issuer,
 audience: null,
 claims: claims,
 expires: DateTime.UtcNow.AddHours(4),
 signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
 );

 var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
 return Ok(new LoginResponse(tokenStr));
 }
}