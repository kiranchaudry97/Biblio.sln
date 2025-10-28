using System.ComponentModel.DataAnnotations;
using Biblio.Models.Entities;

namespace Biblio.Api.DTOs;

public record LoanDto(int Id, int BookId, int MemberId, DateTime? StartDate, DateTime? DueDate, DateTime? ReturnedAt, string? BookTitle, string? MemberName);

public class CreateLoanDto
{
 [Required]
 public int BookId { get; set; }
 [Required]
 public int MemberId { get; set; }
 public DateTime StartDate { get; set; } = DateTime.UtcNow;
 public DateTime? DueDate { get; set; }
}

public class UpdateLoanDto : CreateLoanDto
{
 public DateTime? ReturnedAt { get; set; }
}
