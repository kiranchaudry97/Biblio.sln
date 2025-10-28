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
public class BooksController : ControllerBase
{
 private readonly BiblioDbContext _db;
 public BooksController(BiblioDbContext db) => _db = db;

 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> Get()
 {
 var items = await _db.Books.Include(b => b.Category).Where(b => !b.IsDeleted).ToListAsync();
 var dtos = items.Select(b => new BookDto(b.Id, b.Title, b.Author, b.Isbn, b.CategoryId, b.Category?.Name));
 return Ok(dtos);
 }

 [HttpGet("{id}")]
 [AllowAnonymous]
 public async Task<IActionResult> Get(int id)
 {
 var book = await _db.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
 if (book == null) return NotFound();
 var dto = new BookDto(book.Id, book.Title, book.Author, book.Isbn, book.CategoryId, book.Category?.Name);
 return Ok(dto);
 }

 [HttpPost]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Post([FromBody] CreateBookDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var book = new Book
 {
 Title = dto.Title.Trim(),
 Author = dto.Author?.Trim(),
 Isbn = dto.Isbn?.Trim(),
 CategoryId = dto.CategoryId ??0
 };
 _db.Books.Add(book);
 await _db.SaveChangesAsync();
 var outDto = new BookDto(book.Id, book.Title, book.Author, book.Isbn, book.CategoryId, (await _db.Categories.FindAsync(book.CategoryId))?.Name);
 return CreatedAtAction(nameof(Get), new { id = book.Id }, outDto);
 }

 [HttpPut("{id}")]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Put(int id, [FromBody] UpdateBookDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var exists = await _db.Books.AnyAsync(b => b.Id == id && !b.IsDeleted);
 if (!exists) return NotFound();
 var book = await _db.Books.FindAsync(id);
 if (book == null) return NotFound();
 book.Title = dto.Title.Trim();
 book.Author = dto.Author?.Trim();
 book.Isbn = dto.Isbn?.Trim();
 book.CategoryId = dto.CategoryId ??0;
 _db.Books.Update(book);
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 [Authorize(Roles = "Admin")]
 public async Task<IActionResult> Delete(int id)
 {
 var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
 if (book == null) return NotFound();
 book.IsDeleted = true;
 book.DeletedAt = DateTime.UtcNow;
 _db.Books.Update(book);
 await _db.SaveChangesAsync();
 return NoContent();
 }
}