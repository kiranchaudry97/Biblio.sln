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
public class CategoriesController : ControllerBase
{
 private readonly BiblioDbContext _db;
 public CategoriesController(BiblioDbContext db) => _db = db;

 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> Get()
 {
 var items = await _db.Categories.Where(c => !c.IsDeleted).ToListAsync();
 var dtos = items.Select(c => new CategoryDto(c.Id, c.Name));
 return Ok(dtos);
 }

 [HttpGet("{id}")]
 [AllowAnonymous]
 public async Task<IActionResult> Get(int id)
 {
 var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
 if (cat == null) return NotFound();
 return Ok(new CategoryDto(cat.Id, cat.Name));
 }

 [HttpPost]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Post([FromBody] CreateCategoryDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var category = new Category { Name = dto.Name.Trim() };
 _db.Categories.Add(category);
 await _db.SaveChangesAsync();
 return CreatedAtAction(nameof(Get), new { id = category.Id }, new CategoryDto(category.Id, category.Name));
 }

 [HttpPut("{id}")]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Put(int id, [FromBody] UpdateCategoryDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var exists = await _db.Categories.AnyAsync(c => c.Id == id && !c.IsDeleted);
 if (!exists) return NotFound();
 var cat = await _db.Categories.FindAsync(id);
 if (cat == null) return NotFound();
 cat.Name = dto.Name.Trim();
 _db.Categories.Update(cat);
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 [Authorize(Roles = "Admin")]
 public async Task<IActionResult> Delete(int id)
 {
 var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
 if (cat == null) return NotFound();
 cat.IsDeleted = true;
 cat.DeletedAt = DateTime.UtcNow;
 _db.Categories.Update(cat);
 await _db.SaveChangesAsync();
 return NoContent();
 }
}
