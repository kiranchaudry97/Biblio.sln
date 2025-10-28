using System.ComponentModel.DataAnnotations;
using Biblio.Models.Entities;

namespace Biblio.Api.DTOs;

public record MemberDto(int Id, string FirstName, string LastName, string? Email, string? Phone, string? Address);

public class CreateMemberDto
{
 [Required]
 [StringLength(100)]
 public string FirstName { get; set; } = string.Empty;
 [Required]
 [StringLength(100)]
 public string LastName { get; set; } = string.Empty;
 [EmailAddress]
 public string? Email { get; set; }
 [StringLength(50)]
 public string? Phone { get; set; }
 public string? Address { get; set; }
}

public class UpdateMemberDto : CreateMemberDto { }
