using System.ComponentModel.DataAnnotations;
using Biblio.Models.Entities;

namespace Biblio.Api.DTOs;

public record BookDto(int Id, string Title, string? Author, string? Isbn, int? CategoryId, string? CategoryName);

public class CreateBookDto
{
 [Required]
 [StringLength(200)]
 public string Title { get; set; } = string.Empty;

 [StringLength(200)]
 public string? Author { get; set; }

 [StringLength(20)]
 public string? Isbn { get; set; }

 public int? CategoryId { get; set; }
}

public class UpdateBookDto : CreateBookDto
{
}
