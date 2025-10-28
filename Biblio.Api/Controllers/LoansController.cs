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
public class LoansController : ControllerBase
{
 private readonly BiblioDbContext _db;
 public LoansController(BiblioDbContext db) => _db = db;

 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> Get()
 {
 var items = await _db.Loans.Include(l => l.Book).Include(l => l.Member).Where(l => !l.IsDeleted).ToListAsync();
 var dtos = items.Select(l => new LoanDto(l.Id, l.BookId, l.MemberId, l.StartDate, l.DueDate, l.ReturnedAt, l.Book?.Title, l.Member != null ? $"{l.Member.FirstName} {l.Member.LastName}" : null));
 return Ok(dtos);
 }

 [HttpGet("{id}")]
 [AllowAnonymous]
 public async Task<IActionResult> Get(int id)
 {
 var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Member).FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
 if (loan == null) return NotFound();
 var dto = new LoanDto(loan.Id, loan.BookId, loan.MemberId, loan.StartDate, loan.DueDate, loan.ReturnedAt, loan.Book?.Title, loan.Member != null ? $"{loan.Member.FirstName} {loan.Member.LastName}" : null);
 return Ok(dto);
 }

 [HttpPost]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Post([FromBody] CreateLoanDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 // Basic validation: ensure book and member exist
 var bookExists = await _db.Books.AnyAsync(b => b.Id == dto.BookId && !b.IsDeleted);
 var memberExists = await _db.Members.AnyAsync(m => m.Id == dto.MemberId && !m.IsDeleted);
 if (!bookExists || !memberExists) return BadRequest("Book or Member not found");

 var loan = new Loan { BookId = dto.BookId, MemberId = dto.MemberId, StartDate = dto.StartDate, DueDate = dto.DueDate };
 _db.Loans.Add(loan);
 await _db.SaveChangesAsync();
 var outDto = new LoanDto(loan.Id, loan.BookId, loan.MemberId, loan.StartDate, loan.DueDate, loan.ReturnedAt, (await _db.Books.FindAsync(loan.BookId))?.Title, (await _db.Members.FindAsync(loan.MemberId)) != null ? $"{(await _db.Members.FindAsync(loan.MemberId))!.FirstName} {(await _db.Members.FindAsync(loan.MemberId))!.LastName}" : null);
 return CreatedAtAction(nameof(Get), new { id = loan.Id }, outDto);
 }

 [HttpPut("{id}")]
 [Authorize(Roles = "Admin,Medewerker")]
 public async Task<IActionResult> Put(int id, [FromBody] UpdateLoanDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);
 var exists = await _db.Loans.AnyAsync(l => l.Id == id && !l.IsDeleted);
 if (!exists) return NotFound();
 var loan = await _db.Loans.FindAsync(id);
 if (loan == null) return NotFound();
 loan.BookId = dto.BookId;
 loan.MemberId = dto.MemberId;
 loan.StartDate = dto.StartDate;
 loan.DueDate = dto.DueDate;
 loan.ReturnedAt = dto.ReturnedAt;
 _db.Loans.Update(loan);
 await _db.SaveChangesAsync();
 return NoContent();
 }

 [HttpDelete("{id}")]
 [Authorize(Roles = "Admin")]
 public async Task<IActionResult> Delete(int id)
 {
 var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
 if (loan == null) return NotFound();
 loan.IsDeleted = true;
 loan.DeletedAt = DateTime.UtcNow;
 _db.Loans.Update(loan);
 await _db.SaveChangesAsync();
 return NoContent();
 }
}
