using Biblio.Api.DTOs;
using Biblio.Models.Data;
using Biblio.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Biblio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
 private readonly BiblioDbContext _db;
 public MembersController(BiblioDbContext db) => _db = db;

 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> Get()
 {
 var items = await _db.Members.Where(m => !m.IsDeleted).ToListAsync();
 var dtos = items.Select(m => new MemberDto(m.Id, m.FirstName, m.LastName, m.Email, m.Phone, m.Address));
 return Ok(dtos);
 }

 [HttpGet("{id}")]
 [AllowAnonymous]
 public async Task<IActionResult> Get(int id)
 {
 var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
 if (member == null) return NotFound();
 var dto = new MemberDto(member.Id, member.FirstName, member.LastName, member.Email, member.Phone, member.Address);
 return Ok(dto);
 }

 [HttpPost]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Post([FromBody] CreateMemberDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var member = new Member
 {
 FirstName = dto.FirstName.Trim(),
 LastName = dto.LastName.Trim(),
 Email = dto.Email?.Trim(),
 Phone = dto.Phone?.Trim(),
 Address = dto.Address?.Trim()
 };
 _db.Members.Add(member);
 await _db.SaveChangesAsync();
 var outDto = new MemberDto(member.Id, member.FirstName, member.LastName, member.Email, member.Phone, member.Address);
 return CreatedAtAction(nameof(Get), new { id = member.Id }, outDto);
 }

 [HttpPut("{id}")]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Put(int id, [FromBody] UpdateMemberDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var exists = await _db.Members.AnyAsync(m => m.Id == id && !m.IsDeleted);
 if (!exists) return NotFound();
 var member = await _db.Members.FindAsync(id);
 if (member == null) return NotFound();
 member.FirstName = dto.FirstName.Trim();
 member.LastName = dto.LastName.Trim();
 member.Email = dto.Email?.Trim();
 member.Phone = dto.Phone?.Trim();
 member.Address = dto.Address?.Trim();
 _db.Members.Update(member);
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 [Authorize(Roles = "Admin")]
 public async Task<IActionResult> Delete(int id)
 {
 var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
 if (member == null) return NotFound();
 member.IsDeleted = true;
 member.DeletedAt = DateTime.UtcNow;
 _db.Members.Update(member);
 await _db.SaveChangesAsync();
 return NoContent();
 }
}
